using CS.Components.Damageable;
using CS.Components.Description;
using CS.Components.Skills;
using CS.Components.StatusEffect;
using CS.SlimeFactory;
using Godot;

namespace CS.Components.Damage;

public partial class DamageSystem : NodeSystem
{
    public override void _Ready()
    {
        base._Ready();

        _nodeManager.SignalBus.ReloadCombatDescriptionSignal += OnReloadCombatDescription;
        _nodeManager.SignalBus.UseSkillSignal += OnUseSkill;
        _nodeManager.SignalBus.ProcStatusEffectSignal += OnProcStatusEffect;
    }

    private void OnReloadCombatDescription(Node<DescriptionComponent> node, ref ReloadCombatDescriptionSignal args)
    {
        if (!_nodeManager.TryGetComponent<DamageComponent>(node, out var damageComponent))
            return;

        var combatEffect = $"Damage: {damageComponent.Damage}";
        node.Comp.CombatEffects.Add(combatEffect);
    }

    private void OnProcStatusEffect(Node<StatusEffectComponent> node, ref ProcStatusEffectSignal args)
    {
        if (!_nodeManager.TryGetComponent<DamageComponent>(node, out var damageComponent))
            return;
        
        TryDamageTarget((node.Owner, damageComponent), args.Target);
    }
    
    private void OnUseSkill(Node<SkillComponent> node, ref UseSkillSignal args)
    {
        // Requires a target to damage
        if (args.Target == null)
            return;
        
        if (!_nodeManager.TryGetComponent<DamageComponent>(node, out var damageComponent))
            return;
        
        TryDamageTarget((node, damageComponent), args.Target, args.User);
    }

    /// <summary>
    /// Try to apply damage to a Damageable target
    /// </summary>
    /// <param name="node">Skill being used to damage the defender</param>
    /// <param name="defender">The node being attacked</param>
    /// <param name="attacker">The node attacking the defender</param>
    private void TryDamageTarget(Node<DamageComponent> node, Node defender, Node? attacker = null)
    {
        // Can't damage a target that has no health
        if (!_nodeManager.TryGetComponent<HealthComponent>(defender, out var healthComponent))
            return;
        
        if (_nodeSystemManager.TryGetNodeSystem<DamageableSystem>(out var damageableSystem))
            damageableSystem.TryTakeDamage((defender, healthComponent), node.Comp.Damage);
    }
}