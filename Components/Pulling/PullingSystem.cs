using CS.Components.Grid;
using CS.Components.Interaction;
using CS.SlimeFactory;
using CS.SlimeFactory.Signals;
using Godot;

namespace CS.Components.Pulling;

public partial class PullingSystem : NodeSystem
{
    private const float TileSize = 32f;
    
    [InjectDependency] private readonly GridSystem _gridSystem = null!;
    
    public override void _Ready()
    {
        base._Ready();

        _nodeManager.SignalBus.PullActionSignal += OnPullAction;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        
        /*_nodeManager.NodeQuery<CanPullComponent>(out var dictionary);
        foreach (var (node, comp) in dictionary)
        {
            if (comp.PullingJoint == null)
                continue;

            var pulledNode = GetNode(comp.PullingJoint.NodeB);

            if (pulledNode is not RigidBody2D rigidBody2D)
                continue;
            
            rigidBody2D.Linear
        }*/
    }

    private void OnPullAction(Node<CanInteractComponent> node, ref PullActionSignal args)
    {
        if (!_nodeManager.TryGetComponent<CanPullComponent>(node, out var canPullComponent))
            return;

        if (canPullComponent.PullingJoint != null)
        {
            StopPulling((node, canPullComponent));
            return;
        }

        if (args.pullTarget == null)
            return;
            
        StartPulling((node, canPullComponent), args.pullTarget);
    }

    private void StartPulling(Node<CanPullComponent> node, Node pullTarget)
    {
        if (!_nodeManager.HasComponent<PullableComponent>(pullTarget))
            return;
        
        if (pullTarget is not Node2D pullTarget2D)
            return;

        var dampedSpringJoint2D = new DampedSpringJoint2D();
        dampedSpringJoint2D.Damping = node.Comp.SpringDamping;
        if (_gridSystem.TryGetDistance(node.Owner, pullTarget, out var distance))
            dampedSpringJoint2D.Length = distance.Value * TileSize;
        dampedSpringJoint2D.RestLength = node.Comp.SpringRestLength * TileSize;
        dampedSpringJoint2D.Stiffness = node.Comp.SpringStiffness;
        dampedSpringJoint2D.DisableCollision = node.Comp.DisableCollision;
        // dampedSpringJoint2D.Bias = 2f;
        dampedSpringJoint2D.NodeA = node.Owner.GetPath();
        dampedSpringJoint2D.NodeB = pullTarget2D.GetPath();
        node.Comp.PullingJoint = dampedSpringJoint2D;
        node.Owner.AddChild(dampedSpringJoint2D);

        if (pullTarget is not RigidBody2D rigidBody2D)
            return;
        
        rigidBody2D.SetLinearDamp(5f);
    }

    private void StopPulling(Node<CanPullComponent> node)
    {
        node.Comp.PullingJoint?.QueueFree();
        node.Comp.PullingJoint = null;
    }
}

public partial class PullActionSignal : UserSignalArgs
{
    public Node? pullTarget;

    public PullActionSignal(Node? pullTarget)
    {
        this.pullTarget = pullTarget;
    }
}