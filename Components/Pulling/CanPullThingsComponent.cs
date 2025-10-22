using CS.SlimeFactory;
using Godot;

namespace CS.Components.Pulling;

public partial class CanPullThingsComponent : Component
{
    /// <summary>
    /// Target being pulled, if we are pulling something
    /// </summary>
    public Node? Target; 
    
    /// <summary>
    /// When beginning to pull an object,
    /// set the distance to try and maintain while dragging the object
    /// </summary>
    public float InitialPullDistance;
}