using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Components.Description;

public partial class DescriptionSystem : NodeSystem
{
    public override void _Ready()
    {
        base._Ready();

    }

    public string? TryGetDisplayName(Node node)
    {
        if (!_nodeManager.TryGetComponent<DescriptionComponent>(node, out var descriptionComponent))
            return null;
        
        return descriptionComponent.DisplayName;
    }
    
    public string? TryGetDescription(Node node)
    {
        if (!_nodeManager.TryGetComponent<DescriptionComponent>(node, out var descriptionComponent))
            return null;
        
        return descriptionComponent.Description;
    }

    public CanvasGroup? TryGetAppearance(Node node)
    {
        return node.GetNodeOrNull<CanvasGroup>("CanvasGroup");
    }

    /// <summary>
    /// Return a duplicate of the description effects Array to prevent using it as a pointer
    /// </summary>
    public Array<string> GetEffects(Node node)
    {
        if (!_nodeManager.TryGetComponent<DescriptionComponent>(node, out var descriptionComponent))
            return [];

        descriptionComponent.Effects.Clear();
        var signal = new GetActionEffectsDescriptionSignal();
        _nodeManager.SignalBus.EmitGetActionEffectsDescriptionSignal((node, descriptionComponent), ref signal);

        return descriptionComponent.Effects.Duplicate();
    }
    
    /// <summary>
    /// Return a duplicate of the description costs Array to prevent using it as a pointer
    /// </summary>
    public Array<string> GetCosts(Node node)
    {
        if (!_nodeManager.TryGetComponent<DescriptionComponent>(node, out var descriptionComponent))
            return [];

        descriptionComponent.Costs.Clear();
        var signal = new GetActionCostsDescriptionSignal();
        _nodeManager.SignalBus.EmitGetActionCostsDescriptionSignal((node, descriptionComponent), ref signal);

        return descriptionComponent.Costs.Duplicate();
    }
}