using CS.Components.Player;
using CS.Nodes.UI.Chyron;
using CS.SlimeFactory;
using CS.SlimeFactory.Signals;
using Godot;
using Godot.Collections;

namespace CS.Components.Interaction;

public partial class InteractableComponent : Component
{
    [Export] private CanvasGroup? _canvasGroup;
    [Export] private PhysicsBody2D? _area;
    
    // Having all of this in the component isn't that bad of a sin
    private readonly NodeManager _nodeManager = NodeManager.Instance;
    private readonly NodeSystemManager _nodeSystemManager = NodeSystemManager.Instance;
    
    public override void _Ready()
    {
        base._Ready();

        if (_area == null)
            return;
        
        _area.MouseEntered += OnMouseEntered;
        _area.MouseExited += OnMouseExited;
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
        
        Input.SetDefaultCursorShape(Input.CursorShape.Arrow);
        var signal = new HideInteractOutlineSignal(GetParent());
        _nodeManager.SignalBus.EmitHideInteractOutlineSignal((playerSystem.GetPlayer(), canInteractComponent), ref signal);
    }
}

public partial class InteractWithSignal : HandledSignalArgs
{
    public Node Interactee;

    public InteractWithSignal(Node interactee)
    {
        Interactee = interactee;
    }
}

public partial class GetContextActionsSignal : UserSignalArgs
{
    public Node Interactee;
    public Array<Button> Actions = [];

    public GetContextActionsSignal(Node interactee)
    {
        Interactee = interactee;
    }
}