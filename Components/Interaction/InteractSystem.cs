using CS.Components.Grid;
using CS.Components.Player;
using CS.SlimeFactory;
using Godot;

namespace CS.Components.Interaction;

public partial class InteractSystem : NodeSystem
{
    [InjectDependency] private readonly PlayerManagerSystem _playerManagerSystem = null!;
    [InjectDependency] private readonly GridCoordinateSystem _gridCoordinateSystem = null!;
    
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
        
        if (!@event.IsActionPressed("interact"))
            return;
        
        if (!_nodeManager.TryGetComponent<CanInteractComponent>(_playerManagerSystem.GetPlayer(),
                out var canInteractComponent))
            return;

        if (canInteractComponent.InteractTarget == null)
            return;
        
        if (!_nodeManager.TryGetComponent<InteractableComponent>(canInteractComponent.InteractTarget,
                out var interactableComponent))
            return;
        
        TryInteract((_playerManagerSystem.GetPlayer(), canInteractComponent), (canInteractComponent.InteractTarget, interactableComponent));
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        
        _nodeManager.NodeQuery<CanInteractComponent>(out var dict);
        foreach (var (owner, component) in dict)
        {
            if (component.InteractTarget == null)
                continue;
            
            if (owner is not Node2D user || component.InteractTarget is not Node2D target)
                continue;
            
            var spriteGroup = target.GetNodeOrNull<CanvasGroup>("SpriteGroup");
            
            if (spriteGroup == null)
                continue;

            if (!_gridCoordinateSystem.TryGetDistance(user, target, out var distance))
                continue;
            
            if (distance < component.MaxInteractDistance)
            {
                spriteGroup.Material = InRangeOutline;
                continue;
            }
            
            spriteGroup.Material = OutOfRangeOutline;
        }
    }

    private void OnShowInteractOutline(Node<CanInteractComponent> node, ref ShowInteractOutlineSignal args)
    {
        if (!_nodeManager.HasComponent<InteractableComponent>(args.InteractTarget))
            return;
        
        node.Comp.InteractTarget = args.InteractTarget;
    }
    
    private void OnHideInteractOutline(Node<CanInteractComponent> node, ref HideInteractOutlineSignal args)
    {
        if (node.Comp.InteractTarget == args.InteractTarget)
            node.Comp.InteractTarget = null;
        
        var spriteGroup = args.InteractTarget.GetNodeOrNull<CanvasGroup>("SpriteGroup");

        if (spriteGroup == null)
            return;
        
        spriteGroup.Material = null;
    }

    private void TryInteract(Node<CanInteractComponent> node, Node<InteractableComponent> target)
    {
        if (node.Owner is not Node2D nodeA || target.Owner is not Node2D nodeB)
            return;

        if (!_gridCoordinateSystem.TryGetDistance(nodeA, nodeB, out var distance))
            return;
        
        if (node.Comp.MaxInteractDistance < distance)
            return;
            
        var signal = new InteractWithSignal(node);
        _nodeManager.SignalBus.EmitInteractWithSignal(target, ref signal);
    }
}