using Godot;
using Godot.Collections;

namespace CS.SlimeFactory;

public partial class SignalBus : Node2D
{
    /// <summary>
    /// Making the constructor private prevents the creation of a new <see cref="SignalBus"/>
    /// </summary>
    private SignalBus()
    {
    }

    /// <summary>
    /// Declare that there can only ever be a single instance of the <see cref="SignalBus"/>
    /// </summary>
    public static SignalBus Instance { get; } = new();
}