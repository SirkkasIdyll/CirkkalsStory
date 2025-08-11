using CS.SlimeFactory;
using CS.SlimeFactory.Signals;
using Godot;
using Godot.Collections;

namespace CS.Components.Power;

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

public partial class PowerTransmittedSignal : UserSignalArgs
{
    public float PowerTransmitted;

    public PowerTransmittedSignal(float powerTransmitted)
    {
        PowerTransmitted = powerTransmitted;
    }
}