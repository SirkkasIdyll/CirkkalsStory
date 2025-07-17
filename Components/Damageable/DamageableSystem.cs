using CS.Components.Damage;
using CS.SlimeFactory;

namespace CS.Components.Damageable;

public partial class DamageableSystem : NodeSystem
{
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
        
        // Can't damage a target that isn't damageable
        if (!_nodeManager.TryGetComponent<DamageableComponent>(node, out var damageableComponent))
            return;
        
        var damage = attack.Comp.Damage;

        // Modify damage based on damage category resistance
        damageableComponent.DamageCategoryResistance.TryGetValue(attack.Comp.DamageCategory,
            out var damageCategoryResistance);
        damage *= damageCategoryResistance;
        
        // Modify damage based on damage type resistance
        damageableComponent.DamageTypeResistance.TryGetValue(attack.Comp.DamageType, out var damageTypeResistance);
        damage *= damageTypeResistance;
        
        node.Comp.Health -= (int)damage;
        damageTaken = (int)damage;
        
        var signal = new HealthAlteredSignal();
        _nodeManager.SignalBus.EmitHealthAlteredSignal(node, ref signal);
    }
    
    // TODO: Receive HealthAlteredSignal, check if health hit zero, if so emit mob died signal
}