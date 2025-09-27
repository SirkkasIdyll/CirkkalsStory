using CS.SlimeFactory;
using Godot;

namespace CS.Components.Movement;

/// <summary>
/// Moves the parent body until collision, 
/// at which point an event will be thrown that other systems can react to
/// </summary>
public partial class MoveUntilCollideComponent : Component
{
    /// <summary>
    /// Moves the body along the vector motion
    /// </summary>
    [Export]
    public Vector2 MotionVector;
    
    /// <summary>
    /// Remove this component after collision occurs
    /// </summary>
    [Export]
    public bool RemoveOnCollide;
    
    /// <summary>
    /// Margin beyond the collision body to consider the objects colliding,
    /// which pushes the body away before motion occurs.
    /// This does not affect the movement collision.
    /// </summary>
    [Export]
    public float SafeMarginCollision = 0.08f;

    /// <summary>
    /// When set, will tick down TimeRemaining and stop movement when TimeRemaining reaches zero
    /// </summary>
    [Export]
    public bool Timed;
    
    /// <summary>
    /// Ticks down when Timed is set, stops movement when time reaches zero
    /// </summary>
    [Export]
    public float TimeRemaining;
    
    // Having all of this in the component isn't that bad of a sin
    private readonly NodeManager _nodeManager = NodeManager.Instance;
    private readonly NodeSystemManager _nodeSystemManager = NodeSystemManager.Instance;
    
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (MotionVector == Vector2.Zero)
            return;

        if (GetParent() is not PhysicsBody2D physicsBody2D)
            return;

        if (Timed && TimeRemaining > 0)
            TimeRemaining -= (float)delta;
        
        var kinematicCollision2D = physicsBody2D.MoveAndCollide(MotionVector * (float)delta, safeMargin: SafeMarginCollision);
        
        if (Timed && TimeRemaining <= 0)
        {
            MotionVector = Vector2.Zero;
            return;
        }

        if (kinematicCollision2D == null)
            return;

        if (RemoveOnCollide)
            _nodeManager.RemoveComponent<MoveUntilCollideComponent>(GetParent());
    }
}