using CS.Components.CombatManager;
using CS.Components.Mob;
using CS.Components.Skills;
using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Components.StatusEffect;

public partial class StatusEffectSystem : NodeSystem
{
    public override void _Ready()
    {
        base._Ready();
        
        _nodeManager.SignalBus.StartOfTurnSignal += OnStartOfTurn;
        _nodeManager.SignalBus.UseSkillSignal += OnUseSkill;
    }
    
    /// <summary>
    /// Proc the status effect at the start of the targets turn
    /// </summary>
    /// <param name="node"></param>
    /// <param name="args"></param>
    private void OnStartOfTurn(Node node, ref StartOfTurnSignal args)
    {
        if (!_nodeManager.TryGetComponent<MobComponent>(node, out var mobComponent))
            return;
        
        ProcStatusEffects((node, mobComponent));
    }

    /// <summary>
    /// Apply the status effect to the target
    /// </summary>
    /// <param name="node"></param>
    /// <param name="args"></param>
    private void OnUseSkill(Node<SkillComponent> node, ref UseSkillSignal args)
    {
        if (!_nodeManager.TryGetComponent<StatusEffectApplicatorComponent>(node,
                out var statusEffectApplicatorComponent))
            return;

        if (args.Target != null)
            TryApplyStatusEffect((node, statusEffectApplicatorComponent), args.Target);
    }

    /// <summary>
    /// Go through all of a mob's status effects and proc them
    /// Remove all status effects that are out of duration
    /// </summary>
    /// <param name="node"></param>
    private void ProcStatusEffects(Node<MobComponent> node)
    {
        Array<string> statusEffectsToRemove = [];
        foreach (var statusEffect in node.Component.StatusEffects)
        {
            if (!_nodeManager.TryGetComponent<StatusEffectComponent>(statusEffect.Value, out var statusEffectComponent))
                continue;

            var signal = new ProcStatusEffectSignal(node);
            _nodeManager.SignalBus.EmitProcStatusEffectSignal((statusEffect.Value, statusEffectComponent), ref signal);
            
            // If the status effect is permanent, it can't ever be removed so don't bother checking the duration of it
            if (statusEffectComponent.IsPermanent)
                continue;
            
            statusEffectComponent.StatusDuration -= 1;
            if (statusEffectComponent.StatusDuration <= 0)
                statusEffectsToRemove.Add(statusEffect.Key);
        }

        // Safely remove the status effects outside the first foreach loop
        foreach (var statusEffectName in statusEffectsToRemove)
        {
            node.Component.StatusEffects.Remove(statusEffectName);
        }
    }

    /// <summary>
    /// Attempt to add the status effect to the target's <see cref="MobComponent"/>
    /// </summary>
    /// <param name="node"></param>
    /// <param name="target"></param>
    private void TryApplyStatusEffect(Node<StatusEffectApplicatorComponent> node, Node target)
    {
        // No status effect set to be applied
        if (node.Component.StatusEffect == null)
            return;

        if (!_nodeManager.TryGetComponent<MobComponent>(target, out var mobComponent))
            return;

        var statusEffectName = node.Component.StatusEffect.Name;
        var statusEffect = node.Component.StatusEffect;

        // If the mob isn't currently afflicted with the status effect, apply a duplicate of it
        if (!mobComponent.StatusEffects.ContainsKey(statusEffectName))
        {
            mobComponent.StatusEffects.Add(statusEffect.Name, statusEffect.Duplicate());
        }

        // Get the status effect from the mob and refresh the duration of it
        statusEffect = mobComponent.StatusEffects[statusEffectName];
        if (!_nodeManager.TryGetComponent<StatusEffectComponent>(statusEffect, out var statusEffectComponent))
            return;

        if (statusEffectComponent.StacksDuration)
            statusEffectComponent.StatusDuration += statusEffectComponent.TurnsPerApplication;
        else
            statusEffectComponent.StatusDuration = statusEffectComponent.TurnsPerApplication;
    }
}