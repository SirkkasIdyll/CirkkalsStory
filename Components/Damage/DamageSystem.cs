using CS.Components.Ability;
using CS.Components.CombatManager;
using CS.Components.Damageable;
using CS.Components.Description;
using CS.Components.Mob;
using CS.Components.StatusEffect;
using CS.SlimeFactory;
using Godot;

namespace CS.Components.Damage;

public partial class DamageSystem : NodeSystem
{
    [InjectDependency] private readonly AbilitySystem _abilitySystem = default!;
    [InjectDependency] private readonly DamageableSystem _damageableSystem = default!;
    [InjectDependency] private readonly DescriptionSystem _descriptionSystem = default!;
    
    public override void _Ready()
    {
        base._Ready();

        _nodeManager.SignalBus.ReloadCombatDescriptionSignal += OnReloadCombatDescription;
        _nodeManager.SignalBus.UseActionSignal += OnUseAction;
        _nodeManager.SignalBus.ProcStatusEffectSignal += OnProcStatusEffect;
    }

    private void OnReloadCombatDescription(Node<DescriptionComponent> node, ref ReloadCombatDescriptionSignal args)
    {
        if (!_nodeManager.TryGetComponent<DamageComponent>(node, out var damageComponent))
            return;

        node.Comp.CombatEffects.Add("Category: " + damageComponent.DamageCategory);
        node.Comp.CombatEffects.Add("Type: " + damageComponent.DamageType);
        node.Comp.CombatEffects.Add("Damage: " + damageComponent.Damage);
    }

    private void OnProcStatusEffect(Node<StatusEffectComponent> node, ref ProcStatusEffectSignal args)
    {
        if (!_nodeManager.TryGetComponent<DamageComponent>(node, out var damageComponent))
            return;
        
        TryDamageTarget((node, damageComponent), args.Target, node, out var damageDealt);
        args.Summaries.Add("[b]" + _descriptionSystem.GetDisplayName(args.Target) + "[/b] took " + damageDealt + " damage from [b]" + _descriptionSystem.GetDisplayName(node)+ "[/b].");
    }
    
    private void OnUseAction(Node<MobComponent> node, ref UseActionSignal args)
    {
        if (!_nodeManager.TryGetComponent<DamageComponent>(args.Action, out var damageComponent))
            return;

        foreach (var target in args.Targets)
        {
            TryDamageTarget((args.Action, damageComponent), target, node, out var damageDealt);
            args.Summaries.Add("Dealt " + damageDealt + " damage to [b]" + _descriptionSystem.GetDisplayName(target) + "[/b].");
        }
    }

    /// <summary>
    /// Try to apply damage to a Damageable target
    /// </summary>
    /// <param name="node">Skill being used to damage the defender</param>
    /// <param name="defender">The node being attacked</param>
    /// <param name="attacker">The node attacking the defender</param>
    /// <param name="damageDealt">How much damage is dealt to the target</param>
    public void TryDamageTarget(Node<DamageComponent> node, Node defender, Node attacker, out int damageDealt)
    {
        damageDealt = 0;
        var originalDamage = node.Comp.Damage;
        
        // Can't damage a target that has no health
        if (!_nodeManager.TryGetComponent<HealthComponent>(defender, out var healthComponent))
            return;

        // The attacking source can be a status effect, we'll just check if it isn't
        if (_nodeManager.TryGetComponent<MobComponent>(attacker, out var mobComponent))
        {
            foreach (var abilityName in mobComponent.Abilities)
            {
                if (!_abilitySystem.TryGetAbility(abilityName, out var abilityNode))
                    continue;

                // Reduce damage by a multiplicative amount
                if (_nodeManager.TryGetComponent<DamageDealtMultiplierComponent>(abilityNode,
                        out var damageDealtMultiplierComponent))
                {
                    if (damageDealtMultiplierComponent.DamageDealtCategoryMultiplier.TryGetValue(
                            node.Comp.DamageCategory, out var categoryMultiplier))
                        node.Comp.Damage *= categoryMultiplier;
            
                    if (damageDealtMultiplierComponent.DamageDealtTypeMultiplier.TryGetValue(
                            node.Comp.DamageType, out var typeMultiplier))
                        node.Comp.Damage *= typeMultiplier;
                }

                // Reduce damage by flat amount, but not less than zero
                if (_nodeManager.TryGetComponent<FlatDamageIncreaseComponent>(abilityNode, out var flatDamageIncreaseComponent))
                {
                    if (flatDamageIncreaseComponent.FlatDamageCategoryIncrease.TryGetValue(
                            node.Comp.DamageCategory, out var categoryFlat))
                        node.Comp.Damage += categoryFlat;
            
                    if (flatDamageIncreaseComponent.FlatDamageTypeIncrease.TryGetValue(
                            node.Comp.DamageType, out var typeFlat))
                        node.Comp.Damage += typeFlat;
                }
            }
        }
        
        _damageableSystem.TryTakeDamage((defender, healthComponent), node, out var damageTaken);
        node.Comp.Damage = originalDamage;
        damageDealt = damageTaken;
    }
}