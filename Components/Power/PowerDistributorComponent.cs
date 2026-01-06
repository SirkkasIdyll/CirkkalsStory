using Godot;
using Godot.Collections;
using PC.SlimeFactory;

namespace PC.Components.Power;

/// <summary>
/// Receives power from <see cref="PowerTransmissionComponent"/>
/// Distributes an amount of power to <see cref="PowerCustomerComponent"/>
/// </summary>
public partial class PowerDistributorComponent : Component
{
    /// <summary>
    /// Amount of energy distributed per second
    /// </summary>
    [Export]
    public float DistributionRate;
    
    /// <summary>
    /// How far the power distributed reaches
    /// </summary>
    [Export]
    public float Range;

    public Array<Node> ConnectedTransmitters = [];
    public float PowerToDistribute;
}