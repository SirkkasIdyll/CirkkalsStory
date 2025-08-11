using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Components.Power;

/// <summary>
/// Receives power from <see cref="PowerDistributorComponent"/>
/// </summary>
public partial class PowerCustomerComponent : Component
{
    /// <summary>
    /// Amount of energy consumed per second
    /// </summary>
    [Export]
    public float ConsumptionRate;
    public float PowerConsumed;

    public bool IsSufficientlyPowered;
    
    public Array<Node> ConnectedDistributors;
}