using CS.Components.Player;
using CS.Nodes.UI.ContextMenu;
using CS.SlimeFactory;
using CS.SlimeFactory.Signals;
using Godot;

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
        if (!_nodeSystemManager.TryGetNodeSystem<PlayerManagerSystem>(out var playerManagerSystem))
            return;
        
        var player = playerManagerSystem.TryGetPlayer();
        if (player == null)
            return;

        if (!_nodeManager.TryGetComponent<CanInteractComponent>(player, out var canInteractComponent))
            return;
        
        var signal = new ShowInteractOutlineSignal(GetParent());
        _nodeManager.SignalBus.EmitShowInteractOutlineSignal((player, canInteractComponent), ref signal);
    }
    
    /// <summary>
    /// Remove highlight when mouse exited
    /// </summary>
    private void OnMouseExited()
    {
        if (!_nodeSystemManager.TryGetNodeSystem<PlayerManagerSystem>(out var playerManagerSystem))
            return;
        
        var player = playerManagerSystem.TryGetPlayer();
        if (player == null)
            return;

        if (!_nodeManager.TryGetComponent<CanInteractComponent>(player, out var canInteractComponent))
            return;
        
        Input.SetDefaultCursorShape(Input.CursorShape.Arrow);
        var signal = new HideInteractOutlineSignal(GetParent());
        _nodeManager.SignalBus.EmitHideInteractOutlineSignal((player, canInteractComponent), ref signal);
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

/// <summary>
/// Provides the Interactee and the ContextMenu.
/// To add a new <see cref="ContextMenuAction"/>, append to the enum and add the item to the ContextMenu.
/// </summary>
public partial class GetContextActionsSignal : UserSignalArgs
{
    public Node Interactee;
    public ContextMenu ContextMenu;

    public GetContextActionsSignal(Node interactee)
    {
        Interactee = interactee;
        ContextMenu = new ContextMenu();
        
        // Add initial item to serve as the placeholder for the interact action to guarantee order
        ContextMenu.AddItem("Interact", (int)ContextMenuAction.Interact);
    }
}