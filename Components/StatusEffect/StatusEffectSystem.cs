using Godot;
using Godot.Collections;
using PC.Components.Description;
using PC.Components.Mob;
using PC.SlimeFactory;

namespace PC.Components.StatusEffect;

public partial class StatusEffectSystem : NodeSystem
{
    [InjectDependency] private readonly DescriptionSystem _descriptionSystem = null!;
    
    public override void _Ready()
    {
        base._Ready();
        
        _nodeManager.SignalBus.GetActionEffectsDescriptionSignal += OnGetActionEffectsDescription;
    }

    private void OnGetActionEffectsDescription(Node<DescriptionComponent> node, ref GetActionEffectsDescriptionSignal args)
    {
        if (!_nodeManager.TryGetComponent<StatusEffectApplicatorComponent>(node, out var statusEffectApplicatorComponent))
            return;

        if (statusEffectApplicatorComponent.StatusEffect == null)
            return;

        /*var statusEffectName = _descriptionSystem.GetDisplayName(statusEffectApplicatorComponent.StatusEffect);
        var combatEffect = $"Apply [url={statusEffectApplicatorComponent.StatusEffect.Name}][b]{statusEffectName}[/b][/url]";*/
        // node.Comp.Effects.Add(combatEffect);
    }

    /// <summary>
    /// Go through all of a mob's status effects and proc them
    /// Remove all status effects that are out of duration
    /// </summary>
    private void ProcStatusEffects(Node<MobComponent> node, out Array<string> summaries)
    {
        summaries = [];
        Array<string> statusEffectsToRemove = [];
        foreach (var statusEffect in node.Comp.StatusEffects)
        {
            if (!_nodeManager.TryGetComponent<StatusEffectComponent>(statusEffect.Value, out var statusEffectComponent))
                continue;
            
            var signal = new ProcStatusEffectSignal(node);
            _nodeManager.SignalBus.EmitProcStatusEffectSignal((statusEffect.Value, statusEffectComponent), ref signal);
            summaries.AddRange(signal.Summaries);
            
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
            node.Comp.StatusEffects.Remove(statusEffectName);
        }
    }

    /// <summary>
    /// Attempt to add the status effect to the target's <see cref="MobComponent"/>
    /// </summary>
    private void TryApplyStatusEffect(Node<StatusEffectApplicatorComponent> node, Node target, out Node? appliedEffect)
    {
        appliedEffect = null;
        
        // No status effect set to be applied
        if (node.Comp.StatusEffect == null)
            return;

        if (!_nodeManager.TryGetComponent<MobComponent>(target, out var mobComponent))
            return;

        var statusEffect = node.Comp.StatusEffect;

        // If the mob isn't currently afflicted with the status effect, apply a duplicate of it
        if (!mobComponent.StatusEffects.ContainsKey(statusEffect.Name))
        {
            var dupe = statusEffect.Duplicate();
            mobComponent.StatusEffects.Add(statusEffect.Name, dupe);
            mobComponent.AddChild(dupe);
        }

        // Get the status effect from the mob and refresh the duration of it
        statusEffect = mobComponent.StatusEffects[statusEffect.Name];
        if (!_nodeManager.TryGetComponent<StatusEffectComponent>(statusEffect, out var statusEffectComponent))
            return;

        if (statusEffectComponent.StacksDuration)
            statusEffectComponent.StatusDuration += statusEffectComponent.TurnsPerApplication;
        else
            statusEffectComponent.StatusDuration = statusEffectComponent.TurnsPerApplication;

        appliedEffect = statusEffect;
    }
}