using CS.SlimeFactory;
using CS.SlimeFactory.Signals;
using Godot;

namespace CS.Components.Power;

/// <summary>
/// Generates an amount of power every tick
/// Distributes the power through <see cref="PowerTransmissionComponent"/>
///
/// Working power system is PowerGenerator -> PowerTransmission -> PowerDistributor -> PowerCustomer
/// PowerGen creates dangerous power
/// PowerTrans reduces power to manageable level
/// PowerDistributor serves an amount of power up to a certain amount to customers
/// PowerDistributors break if customer demand is too high
///
/// Should there be high power batteries?
/// Should power trans only transfer power at a certain efficiency?
/// Should power trans have a limit on how much power they can transfer?
/// How do PowerDistributors get fixed?
/// How does anything know what network they're connected to? Does it happen automatically?
/// With magical "wires"? Completely wirelessly but manually connected via a magician?
/// </summary>
public partial class PowerGeneratorComponent : Component
{
    /// <summary>
    /// How much power is generated per second
    /// </summary>
    [Export]
    public float PowerRate;
    public float PowerGenerated;

    /// <summary>
    /// How far the power generated reaches
    /// </summary>
    public float Range;
}

public partial class PowerGeneratedSignal : UserSignalArgs
{
    public float PowerGenerated;
    public double Delta;

    public PowerGeneratedSignal(float powerGenerated, double delta)
    {
        PowerGenerated = powerGenerated;
        Delta = delta;
    }
}