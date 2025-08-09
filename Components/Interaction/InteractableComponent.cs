using CS.Components.Description;
using CS.Components.Player;
using CS.Nodes.UI.Tooltip;
using CS.SlimeFactory;
using CS.SlimeFactory.Signals;
using Godot;

namespace CS.Components.Interaction;

public partial class InteractableComponent : Component
{
    [Export]
    public CanvasGroup? SpriteGroup;

    [Export]
    public PhysicsBody2D? Area;

    [Export]
    public ShaderMaterial OutlineShader = ResourceLoader.Load<ShaderMaterial>("res://Resources/Materials/OutlineCanvasGroup.tres");
    
    private readonly NodeManager _nodeManager = NodeManager.Instance;
    private readonly NodeSystemManager _nodeSystemManager = NodeSystemManager.Instance;
    
    public override void _Ready()
    {
        base._Ready();

        if (Area == null)
            return;
        
        Area.MouseEntered += OnMouseEntered;
        Area.MouseExited += OnMouseExited;
    }
    
    public override void _Input(InputEvent @event)
    {
        if (SpriteGroup == null)
            return;

        if (SpriteGroup.Material != OutlineShader)
            return;

        /*if (!Sprite.IsPixelOpaque(ToLocal(GetGlobalMousePosition())))
            return;*/

        if ((@event is not InputEventMouseButton inputEventMouse || !inputEventMouse.Pressed ||
             inputEventMouse.ButtonIndex != MouseButton.Left) && !Input.IsActionPressed("interact"))
            return;
                
        if (!_nodeSystemManager.TryGetNodeSystem<PlayerManagerSystem>(out var playerSystem))
            return;
                    
        // TODO: Evil hardcoded player interaction
        var signal = new InteractWithSignal(playerSystem.GetPlayer());
        _nodeManager.SignalBus.EmitInteractWithSignal((GetParent(), this), ref signal);
    }

    /// <summary>
    /// Add highlight when mouse entered
    /// </summary>
    private void OnMouseEntered()
    {
        if (!_nodeSystemManager.TryGetNodeSystem<PlayerManagerSystem>(out var playerSystem))
            return;

        if (!_nodeManager.TryGetComponent<CanInteractComponent>(playerSystem.GetPlayer(), out var canInteractComponent))
            return;
        
        var signal = new ShowInteractOutlineSignal(GetParent());
        _nodeManager.SignalBus.EmitShowInteractOutlineSignal((playerSystem.GetPlayer(), canInteractComponent), ref signal);
    }
    
    /// <summary>
    /// Remove highlight when mouse exited
    /// </summary>
    private void OnMouseExited()
    {
        if (!_nodeSystemManager.TryGetNodeSystem<PlayerManagerSystem>(out var playerSystem))
            return;

        if (!_nodeManager.TryGetComponent<CanInteractComponent>(playerSystem.GetPlayer(), out var canInteractComponent))
            return;
        
        var signal = new HideInteractOutlineSignal(GetParent());
        _nodeManager.SignalBus.EmitHideInteractOutlineSignal((playerSystem.GetPlayer(), canInteractComponent), ref signal);
    }

    /*private void CreateTooltip()
    {
        if (!_nodeSystemManager.TryGetNodeSystem<DescriptionSystem>(out var descriptionSystem))
            return;
        
        var tooltip = ResourceLoader.Load<PackedScene>("res://Nodes/UI/Tooltip/CustomTooltip.tscn")
            .Instantiate<CustomTooltip>();
        tooltip.SetTooltipTitle(descriptionSystem.GetDisplayName(GetParent()));
        tooltip.SetTooltipDescription(descriptionSystem.GetDescription(GetParent()));
        AddChild(tooltip);
        var mousePosition = GetViewport().GetMousePosition();
        tooltip.Popup(new Rect2I((int)mousePosition.X - 16, (int)mousePosition.Y - 16, 0, 0));
    }*/
}

public partial class InteractWithSignal : HandledSignalArgs
{
    public Node Interactee;

    public InteractWithSignal(Node interactee)
    {
        Interactee = interactee;
    }
}