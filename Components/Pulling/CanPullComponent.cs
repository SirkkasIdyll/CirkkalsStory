using CS.SlimeFactory;
using Godot;

namespace CS.Components.Pulling;

/// <summary>
/// Manages the spring behavior used when pulling an object
/// </summary>
public partial class CanPullComponent : Component
{
    public DampedSpringJoint2D? PullingJoint = null;
    
    /// <summary>
    /// A ratio between 0 and 1 that controls how much the two nodes try to realign with the spring
    /// </summary>
    [Export(PropertyHint.Range, "0,1,0.01")]
    public float SpringDamping = 1f;

    /// <summary>
    /// The length in tiles the spring rests naturally at. The spring will always try to resize itself to this length.
    /// When set to 0, the spring's rest length will equal its initial length.
    /// </summary>
    [Export]
    public float SpringRestLength = 0.5f;

    /// <summary>
    /// Higher stiffness means the nodes are less capable of influencing the length of the spring
    /// Force applied is equal to stiffness * (restLength - maxLength)
    /// </summary>
    [Export]
    public float SpringStiffness = 20f;

    /// <summary>
    /// When pulling an object, disables collision with that object if true
    /// </summary>
    [Export]
    public bool DisableCollision = true;
}