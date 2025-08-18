using CS.Components.Description;
using CS.Components.Grid;
using CS.Components.Player;
using CS.Nodes.UI.Chyron;
using CS.SlimeFactory;
using Godot;

namespace CS.Components.Interaction;

public partial class InteractSystem : NodeSystem
{
    [InjectDependency] private readonly DescriptionSystem _descriptionSystem = null!;
    [InjectDependency] private readonly PlayerManagerSystem _playerManagerSystem = null!;
    [InjectDependency] private readonly GridSystem _gridSystem = null!;
    
    public ShaderMaterial InRangeOutline = ResourceLoader.Load<ShaderMaterial>("res://Resources/Materials/InRangeOutline.tres");
    public ShaderMaterial OutOfRangeOutline = ResourceLoader.Load<ShaderMaterial>("res://Resources/Materials/OutOfRangeOutline.tres");

    
    public override void _Ready()
    {
        base._Ready();

        _nodeManager.SignalBus.ShowInteractOutlineSignal += OnShowInteractOutline;
        _nodeManager.SignalBus.HideInteractOutlineSignal += OnHideInteractOutline;
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (!_nodeManager.TryGetComponent<CanInteractComponent>(_playerManagerSystem.GetPlayer(),
                out var canInteractComponent))
            return;
        
        if (@event.IsActionPressed("primary_interact"))
            OnPrimaryInteract((_playerManagerSystem.GetPlayer(), canInteractComponent));
        
        if (@event.IsActionPressed("secondary_interact"))
            OnSecondaryInteract((_playerManagerSystem.GetPlayer(), canInteractComponent));
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        ProcessInteractionOutlines();
    }

    private void ProcessInteractionOutlines()
    {
        // Highlight currently focused interact target
        _nodeManager.NodeQuery<CanInteractComponent>(out var dict);
        foreach (var (owner, component) in dict)
        {
            component.Chyron?.SetVisible(false);
            if (component.InteractTarget == null)
                continue;
            
            if (owner is not Node2D user || component.InteractTarget is not Node2D target)
                continue;
            
            var canvasGroup = target.GetNodeOrNull<CanvasGroup>("CanvasGroup");
            
            if (canvasGroup == null)
                continue;

            if (IsObstructed(user, target))
                continue;

            if (!_gridSystem.TryGetDistance(user, target, out var distance))
                continue;

            if (component.Chyron?.Timer.TimeLeft == 0)
            {
                component.Chyron?.SetVisible(true);
            }
            
            Input.SetDefaultCursorShape(Input.CursorShape.PointingHand);
            if (user == target ||  distance < component.MaxInteractDistance)
            {
                canvasGroup.Material = InRangeOutline;
                continue;
            }
            
            canvasGroup.Material = OutOfRangeOutline;
        }
    }

    private void OnPrimaryInteract(Node<CanInteractComponent> node)
    {
        if (node.Comp.InteractTarget == null)
            return;
    
        if (!_nodeManager.TryGetComponent<InteractableComponent>(node.Comp.InteractTarget, out var interactableComponent))
            return;

        if (Input.IsActionPressed("shift_modifier"))
        {
            _descriptionSystem.ShowTooltip(node.Comp.InteractTarget);
            GetViewport().SetInputAsHandled();
            return;
        }

        TryInteract(node, (node.Comp.InteractTarget, interactableComponent));
        GetViewport().SetInputAsHandled();
    }

    private void OnSecondaryInteract(Node<CanInteractComponent> node)
    {
        
    }

    /// <summary>
    /// Adds the target to the list of currently tracked targets to show an outline for
    /// </summary>
    /// <param name="node"></param>
    /// <param name="args"></param>
    private void OnShowInteractOutline(Node<CanInteractComponent> node, ref ShowInteractOutlineSignal args)
    {
        if (!_nodeManager.HasComponent<InteractableComponent>(args.InteractTarget))
            return;
        
        node.Comp.InteractTarget = args.InteractTarget;

        if (!_descriptionSystem.TryGetDisplayName(args.InteractTarget, out var name))
            return;

        node.Comp.Chyron ??= ResourceLoader.Load<PackedScene>("res://Nodes/UI/Chyron/Chyron.tscn")
            .Instantiate<Chyron>();
        if (node.Comp.Chyron == null)
            return;
        
        node.Comp.Chyron.SetText(name);
        if (node.Comp.Chyron.GetParentOrNull<CanvasLayer>() == null)
            CanvasLayer.AddChild(node.Comp.Chyron);
        node.Comp.Chyron.Timer.Start(node.Comp.TimeBeforeChyron);
    }
    
    /// <summary>
    /// Removes the target from the list of currently tracked targets and sets the CanvasGroup material to null
    /// </summary>
    private void OnHideInteractOutline(Node<CanInteractComponent> node, ref HideInteractOutlineSignal args)
    {
        if (node.Comp.InteractTarget == args.InteractTarget)
        {
            node.Comp.InteractTarget = null;
            node.Comp.Chyron?.QueueFree();
            node.Comp.Chyron = null;
        }
        
        var canvasGroup = args.InteractTarget.GetNodeOrNull<CanvasGroup>("CanvasGroup");

        if (canvasGroup == null)
            return;
        
        canvasGroup.Material = null;
    }

    /// <summary>
    /// First check if the nodes are within range of each other
    /// then create a raycast from the origin to the target to check for obstructions
    /// Currently just checks if there's an impassable in the way
    /// </summary>
    private bool InRangeUnobstructed(Node origin, Node target, float range, uint collisionMask = 1)
    {
        if (!_gridSystem.TryGetDistance(origin, target, out var distance))
            return false;

        if (range < distance)
            return false;

        if (origin is not PhysicsBody2D node2DA || target is not PhysicsBody2D node2DB)
            return false;

        var spaceState = GetWorld2D().DirectSpaceState;
        var query = PhysicsRayQueryParameters2D.Create(node2DA.GlobalPosition, node2DB.GlobalPosition, collisionMask, [node2DA.GetRid()]);
        var result = spaceState.IntersectRay(query);
        
        if (result == null || result.Count != 0)
            return false;

        return true;
    }

    private bool IsObstructed(Node origin, Node target, uint collisionMask = 1)
    {
        if (origin is not PhysicsBody2D node2DA || target is not PhysicsBody2D node2DB)
            return false;
        
        var spaceState = GetWorld2D().DirectSpaceState;
        var query = PhysicsRayQueryParameters2D.Create(node2DA.GlobalPosition, node2DB.GlobalPosition, collisionMask, [node2DA.GetRid()]);
        var result = spaceState.IntersectRay(query);
        
        if (result == null || result.Count != 0)
            return true;

        return false;
    }

    private void TryInteract(Node<CanInteractComponent> node, Node<InteractableComponent> target)
    {
        if (node.Owner is not Node2D nodeA || target.Owner is not Node2D nodeB)
            return;

        if (!_gridSystem.TryGetDistance(nodeA, nodeB, out var distance))
            return;
        
        if (node.Comp.MaxInteractDistance < distance)
            return;
        
        var signal = new InteractWithSignal(node);
        _nodeManager.SignalBus.EmitInteractWithSignal(target, ref signal);
    }
}