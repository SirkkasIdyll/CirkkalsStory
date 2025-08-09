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
}

public partial class InteractWithSignal : HandledSignalArgs
{
    public Node Interactee;

    public InteractWithSignal(Node interactee)
    {
        Interactee = interactee;
    }
}