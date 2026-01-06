using Godot;
using Godot.Collections;
using PC.SlimeFactory;
using PC.SlimeFactory.Signals;

namespace PC.Components.Power;

public partial class PowerTransmissionComponent : Component
{
    /// <summary>
    /// Amount of power transmitted per second
    /// </summary>
    [Export]
    public float TransmissionRate;
    public float PowerToTransmit;

    /// <summary>
    /// How far the power transmitted reaches
    /// </summary>
    [Export]
    public float Range;

    public Array<Node> ConnectedGenerators = [];
}

/// <summary>
/// UNUSED
/// </summary>
public partial class PowerTransmittedSignal : UserSignalArgs
{
    public float PowerTransmitted;

    public PowerTransmittedSignal(float powerTransmitted)
    {
        PowerTransmitted = powerTransmitted;
    }
}