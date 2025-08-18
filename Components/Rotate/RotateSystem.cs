using CS.Components.Grid;
using CS.Components.Interaction;
using CS.Components.Player;
using CS.SlimeFactory;
using Godot;

namespace CS.Components.Rotate;

public partial class RotateSystem : NodeSystem
{
    [InjectDependency] private readonly PlayerManagerSystem _playerManagerSystem = null!;
    [InjectDependency] private readonly GridSystem _gridSystem = null!;
    
    public override void _Ready()
    {
        base._Ready();
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        
        if (!_nodeManager.TryGetComponent<CanInteractComponent>(_playerManagerSystem.GetPlayer(),
                out var canInteractComponent))
            return;
        
        if (@event.IsActionPressed("rotate_object"))
            OnRotateObject((_playerManagerSystem.GetPlayer(), canInteractComponent));
    }

    private void OnRotateObject(Node<CanInteractComponent> node)
    {
        if (node.Comp.InteractTarget == null)
            return;
    
        if (!_nodeManager.HasComponent<InteractableComponent>(node.Comp.InteractTarget))
            return;
        
        if (!_nodeManager.HasComponent<RotatableComponent>(node.Comp.InteractTarget))
            return;
        
        if (!Input.IsActionPressed("shift_modifier"))
            _gridSystem.RotateClockwise(node.Comp.InteractTarget);
        else 
            _gridSystem.RotateCounterClockwise(node.Comp.InteractTarget);
    }
}