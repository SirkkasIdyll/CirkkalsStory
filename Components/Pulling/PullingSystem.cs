using CS.Components.Grid;
using CS.Components.Interaction;
using CS.Components.Movement;
using CS.Nodes.UI.ContextMenu;
using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Components.Pulling;

public partial class PullingSystem : NodeSystem
{
    [InjectDependency] private readonly GridSystem _gridSystem = null!;
    [InjectDependency] private readonly InteractSystem _interactSystem = null!;
    
    public override void _Ready()
    {
        base._Ready();
        
        _nodeManager.SignalBus.GetContextActionsSignal += OnGetContextActions;
        _nodeManager.SignalBus.MovementAttemptSignal += OnMovementAttempt;
    }

    private void OnContextActionIndexPressed(ContextMenu contextMenu, int index)
    {
        if (!contextMenu.IndexIdMatchesAction(index, ContextMenuAction.Pull)
            && !contextMenu.IndexIdMatchesAction(index, ContextMenuAction.StopPull))
            return;
        
        var dictionary = (Dictionary<string, Node>)contextMenu.GetItemMetadata(index);
        var node = dictionary["node"];
        var interactee = dictionary["interactee"];

        if (!_nodeManager.TryGetComponent<CanPullThingsComponent>(interactee, out var canPullThingsComponent))
            return;

        if (!_nodeManager.TryGetComponent<PullableComponent>(node, out var pullableComponent))
            return;

        if (contextMenu.IndexIdMatchesAction(index, ContextMenuAction.Pull))
            TryPull((interactee, canPullThingsComponent), (node, pullableComponent));
        
        if (contextMenu.IndexIdMatchesAction(index, ContextMenuAction.StopPull))
            TryStopPull((interactee, canPullThingsComponent), (node, pullableComponent));
    }

    private void OnGetContextActions(Node<InteractableComponent> node, ref GetContextActionsSignal args)
    {
        if (!_nodeManager.TryGetComponent<PullableComponent>(node, out var pullableComponent))
            return;

        var contextMenu = args.ContextMenu;
        args.ContextMenu.IndexPressed += index => OnContextActionIndexPressed(contextMenu, (int)index);
        
        if (pullableComponent.IsBeingPulled && pullableComponent.PulledBy == args.Interactee)
            contextMenu.AddItem("Stop pulling", (int)ContextMenuAction.StopPull);
        else
            contextMenu.AddItem("Start pulling", (int)ContextMenuAction.Pull);
        
        var index = contextMenu.GetItemCount() - 1;
        contextMenu.SetItemMetadata(index, new Dictionary<string, Node>()
        {
            { "node", node },
            { "interactee", args.Interactee }
        });
        
        if (!_nodeManager.HasComponent<CanPullThingsComponent>(args.Interactee))
            contextMenu.SetItemDisabled(index, true);
    }

    /// <summary>
    /// When a character wants to move, attempt to release the player from being pulled
    /// If it fails, cancel the movement attempt
    /// </summary>
    private void OnMovementAttempt(Node<MovementComponent> node, ref MovementAttemptSignal args)
    {
        if (!_nodeManager.TryGetComponent<PullableComponent>(node, out var pullableComponent))
            return;

        if (!pullableComponent.IsBeingPulled)
            return;

        if (pullableComponent.PulledBy == null)
            return;

        if (!_nodeManager.TryGetComponent<CanPullThingsComponent>(pullableComponent.PulledBy,
                out var canPullThingsComponent))
            return;

        if (!TryStopPull((pullableComponent.PulledBy, canPullThingsComponent), (node, pullableComponent)))
            args.Canceled = true;
    }

    /// <summary>
    /// When attempting to pull something,
    /// check if the target is in range,
    /// or if there are any other systems preventing pulling
    /// </summary>
    public bool CanPull(Node<CanPullThingsComponent> node, Node<PullableComponent> pullable)
    {
        if (!_nodeManager.TryGetComponent<CanInteractComponent>(node, out var canInteractComponent))
            return false;

        if (!_interactSystem.InRangeUnobstructed(node, pullable, canInteractComponent.MaxInteractDistance))
            return false;

        var signal = new CanPullSignal(pullable);
        _nodeManager.SignalBus.EmitCanPullSignal(node, ref signal);

        if (signal.Canceled)
            return false;

        return true;
    }

    /// <summary>
    /// When attempting to stop pulling something,
    /// check if the target is being pulled by the puller, and vice versa, 
    /// and then check other systems that might prevent pulling from being stopped 
    /// </summary>
    public bool CanStopPulling(Node<CanPullThingsComponent> node, Node<PullableComponent> pullable)
    {
        if (node.Comp.Target != pullable.Owner || pullable.Comp.PulledBy != node.Owner)
            return false;
        
        var signal = new CanStopPullingSignal(pullable);
        _nodeManager.SignalBus.EmitCanStopPullingSignal(node, ref signal);

        if (signal.Canceled)
            return false;

        return true;
    }
    
    /// <summary>
    /// Attempt to start pulling a chosen target
    /// </summary>
    public bool TryPull(Node<CanPullThingsComponent> puller, Node<PullableComponent> pullable)
    {
        if (!CanPull(puller, pullable))
            return false;
        
        // If we're already pulling another target, try to stop pulling it
        if (!TryStopPull(puller))
            return false;

        puller.Comp.Target = pullable;
        if (_gridSystem.TryGetDistance(puller, pullable, out var distance))
            puller.Comp.InitialPullDistance = distance.Value; 
        pullable.Comp.IsBeingPulled = true;
        pullable.Comp.PulledBy = puller;
        return true;
    }

    /// <summary>
    /// Stop pulling whatever you happen to be pulling
    /// </summary>
    public bool TryStopPull(Node<CanPullThingsComponent> puller)
    {
        if (puller.Comp.Target == null)
            return true;
        
        if (!_nodeManager.TryGetComponent<PullableComponent>(puller.Comp.Target, out var pullableComponent))
            return false;

        return TryStopPull(puller, (puller.Comp.Target, pullableComponent));
    }

    /// <summary>
    /// Attempt to stop pulling a given target
    /// </summary>
    public bool TryStopPull(Node<CanPullThingsComponent> puller, Node<PullableComponent> pullable)
    {
        if (!CanStopPulling(puller, pullable))
            return false;
        
        puller.Comp.Target = null;
        puller.Comp.InitialPullDistance = 0;
        pullable.Comp.IsBeingPulled = false;
        pullable.Comp.PulledBy = null;
        return true;
    }
}