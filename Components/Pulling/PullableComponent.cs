using CS.Components.Grid;
using CS.SlimeFactory;
using Godot;

namespace CS.Components.Pulling;

public partial class PullableComponent : Component
{
    /// <summary>
    /// Whether we are currently being pulled or not
    /// </summary>
    public bool IsBeingPulled;

    /// <summary>
    /// Who is currently pulling us
    /// </summary>
    public Node? PulledBy;
    
    // Having all of this in the component isn't that bad of a sin
    private readonly NodeManager _nodeManager = NodeManager.Instance;
    private readonly NodeSystemManager _nodeSystemManager = NodeSystemManager.Instance;
    private GridSystem _gridSystem = null!;

    public override void _Ready()
    {
        base._Ready();

        if (_nodeSystemManager.TryGetNodeSystem<GridSystem>(out var gridSystem))
            _gridSystem = gridSystem;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (GetParent() is not PhysicsBody2D physicsBody2D)
            return;
        
        if (!IsBeingPulled)
            return;

        if (PulledBy == null)
            return;

        if (!_nodeManager.TryGetComponent<CanPullThingsComponent>(PulledBy, out var canPullThingsComponent))
            return;

        if (!_gridSystem.TryGetDistanceVector(PulledBy, GetParent(), out var distanceVector))
            return;

        if (distanceVector.Value.Length() < canPullThingsComponent.InitialPullDistance)
            return;
        
        physicsBody2D.MoveAndCollide(distanceVector.Value * (float)delta * 60f);
    }
}