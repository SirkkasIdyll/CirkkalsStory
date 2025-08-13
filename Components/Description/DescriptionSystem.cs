using System.Diagnostics.CodeAnalysis;
using CS.Nodes.UI.Tooltip;
using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Components.Description;

public partial class DescriptionSystem : NodeSystem
{
    private PackedScene _customTooltipScene =
        ResourceLoader.Load<PackedScene>("res://Nodes/UI/Tooltip/CustomTooltip.tscn");
    
    public override void _Ready()
    {
        base._Ready();

    }

    public bool TryGetDisplayName(Node node, [NotNullWhen(true)] out string? name)
    {
        name = null;
        
        if (!_nodeManager.TryGetComponent<DescriptionComponent>(node, out var descriptionComponent))
            return false;
        
        name = descriptionComponent.DisplayName;
        return true;
    }
    
    public bool TryGetDescription(Node node, [NotNullWhen(true)] out string? description)
    {
        description = null;
        
        if (!_nodeManager.TryGetComponent<DescriptionComponent>(node, out var descriptionComponent))
            return false;
        
        description = descriptionComponent.Description;
        return true;
    }

    public void ShowTooltip(Node node)
    {
        if (node is not Node2D node2D)
            return;
        
        if (!_nodeManager.TryGetComponent<DescriptionComponent>(node, out var descriptionComponent))
            return;

        var customTooltip = _customTooltipScene.Instantiate<CustomTooltip>();
        customTooltip.SetTooltipTitle(descriptionComponent.DisplayName);
        customTooltip.SetTooltipDescription(descriptionComponent.Description);
        customTooltip.SetTooltipBulletpoints(descriptionComponent.DetailedSummary);
        CanvasLayer.AddChild(customTooltip);
        var mousePosition = GetViewport().GetMousePosition();
        customTooltip.SetPosition(new Vector2I((int)mousePosition.X + 16, (int)mousePosition.Y + 16));
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