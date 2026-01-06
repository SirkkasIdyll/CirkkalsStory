using System.Diagnostics.CodeAnalysis;
using Godot;
using Godot.Collections;
using PC.Components.Description;
using PC.Components.Grid;
using PC.Components.Player;
using PC.Nodes.UI.Chyron;
using PC.Nodes.UI.ContextMenu;
using PC.SlimeFactory;

namespace PC.Components.Interaction;

public partial class InteractSystem : NodeSystem
{
    [InjectDependency] private readonly DescriptionSystem _descriptionSystem = null!;
    [InjectDependency] private readonly GridSystem _gridSystem = null!;
    [InjectDependency] private readonly PlayerManagerSystem _playerManagerSystem = null!;

    private ShaderMaterial _inRangeOutline = ResourceLoader.Load<ShaderMaterial>("res://Resources/Materials/InRangeOutline.tres");
    private ShaderMaterial _outOfRangeOutline = ResourceLoader.Load<ShaderMaterial>("res://Resources/Materials/OutOfRangeOutline.tres");
    
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        ProcessInteractionOutlines();
    }

    public override void _Ready()
    {
        base._Ready();

        _nodeManager.SignalBus.GetContextActionsSignal += OnGetContextActions;
        _nodeManager.SignalBus.HideInteractOutlineSignal += OnHideInteractOutline;
        _nodeManager.SignalBus.ShowInteractOutlineSignal += OnShowInteractOutline;
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

    /// <summary>
    /// When a context menu action is pressed, check if it's the interact option,
    /// if it was, try to interact with the node.
    /// </summary>
    private void OnContextActionIndexPressed(ContextMenu contextMenu, int index)
    {
        if (!contextMenu.IndexIdMatchesAction(index, ContextMenuAction.Interact))
            return;

        var dictionary = (Dictionary<string, Node>)contextMenu.GetItemMetadata(index);
        var node = dictionary["node"];
        var interactee = dictionary["interactee"];
        
        if (!_nodeManager.TryGetComponent<InteractableComponent>(node, out var interactableComponent))
            return;
        
        if (!_nodeManager.TryGetComponent<CanInteractComponent>(interactee, out var canInteractComponent))
            return;
        
        TryInteract((interactee, canInteractComponent), (node, interactableComponent));
    }

    /// <summary>
    /// Subscribe to the context menu and update the interact option in the context menu
    /// </summary>
    private void OnGetContextActions(Node<InteractableComponent> node, ref GetContextActionsSignal args)
    {
        // Because anything that gets a context action is interactable,
        // we don't actually have to check if it is.
        var contextMenu = args.ContextMenu;
        args.ContextMenu.IndexPressed += index => OnContextActionIndexPressed(contextMenu, (int)index);
        
        if (_descriptionSystem.TryGetDisplayName(node, out var name))
            contextMenu.SetItemText(0, name);
        
        if (_descriptionSystem.TryGetSprite(node, out var sprite2D))
            contextMenu.SetItemIcon(0, sprite2D.Texture);

        contextMenu.SetItemMetadata(0, new Dictionary<string, Node>()
        {
            { "node", node.Owner },
            { "interactee", args.Interactee }
        });
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

    /// <summary>
    /// Get context actions for a specific interactable,
    /// or get all nearby interactable nodes in a context menu
    /// </summary>
    private void OnSecondaryInteract(Node<CanInteractComponent> node)
    {
        if (node.Comp.InteractTarget != null &&
            _nodeManager.TryGetComponent<InteractableComponent>(node.Comp.InteractTarget, out var interactableComponent))
        {
            var signal = new GetContextActionsSignal(node);
            _nodeManager.SignalBus.EmitGetContextActionsSignal((node.Comp.InteractTarget, interactableComponent), ref signal);
            CanvasLayer.AddChild(signal.ContextMenu);
            return;
        }

        CreateNearbyInteractableNodesContextMenu();
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

    /// <summary>
    /// Check for obstruction between two nodes
    /// </summary>
    public bool IsObstructed(Node origin, Node target, [NotNullWhen(true)]out Dictionary? collisionInfo, uint collisionMask = 1)
    {
        collisionInfo = null;
        
        if (origin is not PhysicsBody2D node2DA || target is not PhysicsBody2D node2DB)
            return false;
        
        var spaceState = GetWorld2D().DirectSpaceState;
        var query = PhysicsRayQueryParameters2D.Create(node2DA.GlobalPosition, node2DB.GlobalPosition, collisionMask, [node2DA.GetRid()]);
        var result = spaceState.IntersectRay(query);

        if (result != null && result.Count != 0)
        {
            collisionInfo = result;
            return true;
        }

        return false;
    }
    
    /// <summary>
    /// Check for obstruction between a node and a global position
    /// </summary>
    public bool IsObstructed(Node origin, Vector2 target, [NotNullWhen(true)]out Dictionary? collisionInfo, uint collisionMask = 1)
    {
        collisionInfo = null;
        
        if (origin is not PhysicsBody2D node2DA)
            return false;
        
        var spaceState = GetWorld2D().DirectSpaceState;
        var query = PhysicsRayQueryParameters2D.Create(node2DA.GlobalPosition, target, collisionMask, [node2DA.GetRid()]);
        var result = spaceState.IntersectRay(query);

        if (result != null && result.Count != 0)
        {
            collisionInfo = result;
            return true;
        }

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

    private void CreateNearbyInteractableNodesContextMenu()
    {
        var player = _playerManagerSystem.TryGetPlayer();
        if (player == null)
            return;
        
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
        player.GetParent().AddChild(area2D);
        area2D.SetGlobalPosition(GetGlobalMousePosition());
        
        // After a random specified amount of time, get the overlapping bodies and narrow them down to interactables
        // then list them for selection to be interacted with
        var timer = GetTree().CreateTimer(0.1f);
        timer.Timeout += () =>
        {
            Array<Node2D> interactableBodies = [];
            foreach (var body in area2D.GetOverlappingBodies())
            {
                if (_nodeManager.HasComponent<InteractableComponent>(body) && body.IsVisibleInTree())
                    interactableBodies.Add(body);
            }

            if (interactableBodies.Count == 0)
            {
                area2D.QueueFree();
                return;
            }

            var contextMenu = new ContextMenu();
            contextMenu.IndexPressed += index => OnContextActionIndexPressed(contextMenu, (int)index);
            foreach (var node2D in interactableBodies)
            {
                _descriptionSystem.TryGetDisplayName(node2D, out var name);
                _descriptionSystem.TryGetSprite(node2D, out var sprite);

                if (name == null)
                    return;
                
                if (sprite == null)
                    contextMenu.AddItem(name, (int)ContextMenuAction.Interact);
                else
                    contextMenu.AddIconItem(sprite.Texture, name, (int)ContextMenuAction.Interact);
                
                var index = contextMenu.GetItemCount() - 1;
                contextMenu.SetItemMetadata(index, new Dictionary<string, Node>()
                {
                    { "node", node2D },
                    { "interactee", player }
                });
            }
            CanvasLayer.AddChild(contextMenu);
            area2D.QueueFree();
        };
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

            if (IsObstructed(user, target, out _))
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
}