using CS.Components.Damageable;
using CS.Components.Skills;
using CS.SlimeFactory;
using Godot;

namespace CS.Components.Damage;

public partial class DamageSystem : NodeSystem
{
    public override void _Ready()
    {
        base._Ready();

        _nodeManager.SignalBus.UseSkillSignal += OnUseSkill;
        // _nodeManager.SignalBus.DamageAttemptSignal += TryDamageTarget;
        // signal for attempting to damage target
    }

    private void OnUseSkill(Node<SkillComponent> node, UseSkillSignal args)
    {
        if (!_nodeManager.TryGetComponent<DamageComponent>(node, out var damageComponent))
            return;
        
        TryDamageTarget((node, damageComponent), args.Target);
    }

    private void TryDamageTarget(Node<DamageComponent> node, Node? defender)
    {
        if (defender == null)
            return;

        // Can't damage a target that isn't damageable
        if (!_nodeManager.HasComponent<DamageableComponent>(defender))
            return;

        // Can't damage a target that has no health
        if (!_nodeManager.TryGetComponent<HealthComponent>(defender, out var targetHealthComponent))
            return;
        
        targetHealthComponent.AlterHealth(-node.Component.Damage);
    }

    /*public string DescribeEffect()
    {
        return $"Damage: {Damage}";
    }*/
}