using CS.Components.Ability;
using CS.Components.Damage;
using CS.Components.Mob;
using CS.SlimeFactory;

namespace CS.Components.Damageable;

public partial class DamageableSystem : NodeSystem
{
    [InjectDependency] private readonly AbilitySystem _abilitySystem = default!;
    
    public override void _Ready()
    {
        base._Ready();

        _nodeManager.SignalBus.HealthAlteredSignal += OnHealthAltered;
    }

    private void OnHealthAltered(Node<HealthComponent> node, ref HealthAlteredSignal args)
    {
        if (node.Comp.Health <= 0)
        {
            var signal = new MobDiedSignal();
            _nodeManager.SignalBus.EmitMobDiedSignal(node, ref signal);
        }
    }

    /// <summary>
    /// Try to make the node take damage
    /// </summary>
    /// <param name="node">The mob taking damage</param>
    /// <param name="attack">The attack being aimed at the target</param>
    /// <param name="damageTaken">How much damage is taken after calculations</param>

    public void TryTakeDamage(Node<HealthComponent> node, Node<DamageComponent> attack, out int damageTaken)
    {
        damageTaken = 0;
        var damage = attack.Comp.Damage;
        
        // Can't damage a target that isn't damageable
        if (!_nodeManager.TryGetComponent<DamageableComponent>(node, out var damageableComponent))
            return;
        
        // Modify damage based on damage category resistance
        damageableComponent.DamageCategoryResistance.TryGetValue(attack.Comp.DamageCategory,
            out var damageCategoryResistance);
        damage *= damageCategoryResistance;
        
        // Modify damage based on damage type resistance
        damageableComponent.DamageTypeResistance.TryGetValue(attack.Comp.DamageType, out var damageTypeResistance);
        damage *= damageTypeResistance;

        if (_nodeManager.TryGetComponent<MobComponent>(node, out var mobComponent))
        {
            foreach (var abilityName in mobComponent.Abilities)
            {
                if (!_abilitySystem.TryGetAbility(abilityName, out var abilityNode))
                    continue;

                // Reduce damage by a multiplicative amount
                if (_nodeManager.TryGetComponent<DamageResistMultiplierComponent>(abilityNode,
                        out var damageResistMultiplierComponent))
                {
                    if (damageResistMultiplierComponent.DamageCategoryResistanceMultiplier.TryGetValue(
                            attack.Comp.DamageCategory, out var categoryMultiplier))
                        damage /= categoryMultiplier;
            
                    if (damageResistMultiplierComponent.DamageTypeResistanceMultiplier.TryGetValue(
                            attack.Comp.DamageType, out var typeMultiplier))
                        damage /= typeMultiplier;
                }

                // Reduce damage by flat amount, but not less than zero
                if (_nodeManager.TryGetComponent<FlatDamageResistComponent>(abilityNode, out var flatDamageResistComponent))
                {
                    if (flatDamageResistComponent.FlatDamageCategoryResistance.TryGetValue(
                            attack.Comp.DamageCategory, out var categoryFlat))
                        damage = float.Max(damage - categoryFlat, 0);
            
                    if (flatDamageResistComponent.FlatDamageTypeResistance.TryGetValue(
                            attack.Comp.DamageType, out var typeFlat))
                        damage = float.Max(damage - typeFlat, 0);
                }
            }
        }
        
        node.Comp.Health -= (int)damage;
        damageTaken = (int)damage;
        
        var signal = new HealthAlteredSignal();
        _nodeManager.SignalBus.EmitHealthAlteredSignal(node, ref signal);
    }
    
    // TODO: Receive HealthAlteredSignal, check if health hit zero, if so emit mob died signal
}