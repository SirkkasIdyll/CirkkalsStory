using CS.Components.Grid;
using CS.Components.Interaction;
using CS.SlimeFactory;

namespace CS.Components.Sitting;

public partial class SittingSystem : NodeSystem
{
    [InjectDependency] private readonly GridSystem _gridSystem = null!;
    
    public override void _Ready()
    {
        base._Ready();

        _nodeManager.SignalBus.InteractWithSignal += OnInteractWith;
    }

    public void OnInteractWith(Node<InteractableComponent> node, ref InteractWithSignal args)
    {
        if (!_nodeManager.TryGetComponent<SeatComponent>(node, out var seatComponent))
            return;

        _gridSystem.ToggleAnchored(node);
        
        /*if (args.Interactee is not CharacterBody2D interacteeBody)
            return;
        
        if (node.Owner is not RigidBody2D physicsBody2D)
            return;
        
        interacteeBody.Position = physicsBody2D.Position;
        physicsBody2D.SetLockRotationEnabled(true);
        var joint = new PinJoint2D();
        joint.NodeA = physicsBody2D.GetPath();
        joint.NodeB = interacteeBody.GetPath();
        GetTree().Root.AddChild(joint);*/
    }
}
