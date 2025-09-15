using CS.Components.Description;
using CS.Components.Grid;
using CS.Components.Player;
using CS.Nodes.UI.Chyron;
using CS.Nodes.UI.ContextButtonList;
using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Components.Interaction;

public partial class InteractSystem : NodeSystem
{
    [InjectDependency] private readonly DescriptionSystem _descriptionSystem = null!;
    [InjectDependency] private readonly PlayerManagerSystem _playerManagerSystem = null!;
    [InjectDependency] private readonly GridSystem _gridSystem = null!;
    
    private ShaderMaterial _inRangeOutline = ResourceLoader.Load<ShaderMaterial>("res://Resources/Materials/InRangeOutline.tres");
    private ShaderMaterial _outOfRangeOutline = ResourceLoader.Load<ShaderMaterial>("res://Resources/Materials/OutOfRangeOutline.tres");
    private PackedScene _contextButtonList = ResourceLoader.Load<PackedScene>("res://Nodes/UI/ContextButtonList/ContextButtonList.tscn");
    
    public override void _Ready()
    {
        base._Ready();

        _nodeManager.SignalBus.ShowInteractOutlineSignal += OnShowInteractOutline;
        _nodeManager.SignalBus.HideInteractOutlineSignal += OnHideInteractOutline;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        
        var player = _playerManagerSystem.TryGetPlayer();
        if (player == null)
            return;

        if (!_nodeManager.TryGetComponent<CanInteractComponent>(player, out var canInteractComponent))
            return;
        
        if (@event.IsActionPressed("primary_interact"))
            OnPrimaryInteract((player, canInteractComponent));
        
        if (@event.IsActionPressed("secondary_interact"))
            OnSecondaryInteract((player, canInteractComponent));
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
                canvasGroup.Material = _inRangeOutline;
                continue;
            }
            
            canvasGroup.Material = _outOfRangeOutline;
        }
    }

    /// <summary>
    /// Interact with the target being focused on in the CanInteractComponent
    /// </summary>
    private void OnPrimaryInteract(Node<CanInteractComponent> node)
    {
        if (node.Comp.InteractTarget == null)
            return;
    
        if (!_nodeManager.TryGetComponent<InteractableComponent>(node.Comp.InteractTarget, out var interactableComponent))
            return;
        
        OnPrimaryInteract(node, (node.Comp.InteractTarget, interactableComponent));
    }

    /// <summary>
    /// Interact with the given target if it happens to be interactable
    /// </summary>
    private void OnPrimaryInteract(Node<CanInteractComponent> node, Node target)
    {
        if (!_nodeManager.TryGetComponent<InteractableComponent>(target, out var interactableComponent))
            return;
        
        OnPrimaryInteract(node, (target, interactableComponent));
    }

    /// <summary>
    /// Interact with a known interactable target
    /// </summary>
    private void OnPrimaryInteract(Node<CanInteractComponent> node, Node<InteractableComponent> interactable)
    {
        if (Input.IsActionPressed("shift_modifier"))
        {
            _descriptionSystem.ShowTooltip(interactable);
            return;
        }

        TryInteract(node, interactable);
    }

    private void OnSecondaryInteract(Node<CanInteractComponent> node)
    {
        if (node.Comp.InteractTarget != null &&
            _nodeManager.TryGetComponent<InteractableComponent>(node.Comp.InteractTarget, out var interactableComponent))
        {
            var signal = new GetContextActionsSignal(node);
            _nodeManager.SignalBus.EmitGetContextActionsSignal((node.Comp.InteractTarget, interactableComponent), ref signal);
            
            var nodeButtonList = _contextButtonList.Instantiate<ContextButtonListSystem>();
            nodeButtonList.Setup(signal.Actions);
            CanvasLayer.AddChild(nodeButtonList);
            nodeButtonList.SetPosition(GetViewport().GetMousePosition());
            return;
        }
        
        // Create an area that detects nodes on all layers
        var area2D = new Area2D();
        area2D.SetCollisionMask(0b11111111);
        
        // Area is a square the size of one standard tile
        var collionShape2D = new CollisionShape2D();
        var rectangleShape2D = new RectangleShape2D();
        rectangleShape2D.SetSize(new Vector2(32, 32));
        collionShape2D.Shape = rectangleShape2D;
        area2D.AddChild(collionShape2D);
        
        // Adds the area to the scene at the mouse location
        _playerManagerSystem.TryGetPlayer()?.GetParent().AddChild(area2D);
        area2D.SetGlobalPosition(GetGlobalMousePosition());
        
        // After a random specified amount of time, get the overlapping bodies and narrow them down to interactables
        // then list them for selection to be interacted with
        var timer = GetTree().CreateTimer(0.1f);
        timer.Timeout += () =>
        {
            Array<Node2D> interactableBodies = [];
            foreach (var body in area2D.GetOverlappingBodies())
            {
                if (_nodeManager.HasComponent<InteractableComponent>(body) && body.IsVisible())
                    interactableBodies.Add(body);
            }

            if (interactableBodies.Count == 0)
            {
                area2D.QueueFree();
                return;
            }
            
            var nodeButtonList = _contextButtonList.Instantiate<ContextButtonListSystem>();
            nodeButtonList.Setup(interactableBodies);
            CanvasLayer.AddChild(nodeButtonList);
            nodeButtonList.SetPosition(GetViewport().GetMousePosition());
            area2D.QueueFree();
        };
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
    public bool InRangeUnobstructed(Node origin, Node target, float range, uint collisionMask = 1)
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

    public bool IsObstructed(Node origin, Node target, uint collisionMask = 1)
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

    public void TryInteract(Node<CanInteractComponent> node, Node<InteractableComponent> target)
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