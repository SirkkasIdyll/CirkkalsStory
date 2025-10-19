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
}