using CS.Components.Player;
using CS.SlimeFactory;
using Godot;

namespace CS.Components.Interaction;

public partial class InteractSystem : NodeSystem
{
    [InjectDependency] private readonly PlayerManagerSystem _playerManagerSystem = null!;
    
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
        
        if ((@event is not InputEventMouseButton inputEventMouse || !inputEventMouse.Pressed ||
             inputEventMouse.ButtonIndex != MouseButton.Left) && !Input.IsActionPressed("interact"))
            return;

        // TODO: Kind of evil hardcoded player interaction
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

            if (target.GlobalPosition.DistanceTo(user.GlobalPosition) < component.MaxInteractDistance)
                spriteGroup.Material = InRangeOutline;
            else
                spriteGroup.Material = OutOfRangeOutline;

        }
    }

    /// Something something process CanInteractComponent, get range, set color
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
        
        var distance = nodeA.GlobalPosition.DistanceTo(nodeB.GlobalPosition);
        
        if (node.Comp.MaxInteractDistance < distance)
            return;
            
        var signal = new InteractWithSignal(node);
        _nodeManager.SignalBus.EmitInteractWithSignal(target, ref signal);
    }
}