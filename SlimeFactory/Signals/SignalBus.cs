namespace CS.SlimeFactory.Signals;

/// <summary>
/// This has Node2D because it needs access to Godot's Signal functions despite not being put into the SceneTree
/// </summary>
public partial class SignalBus
{
    /// <summary>
    /// Declare that there can only ever be a single instance of the <see cref="Signals.SignalBus"/>
    /// </summary>
    public static SignalBus Instance { get; } = new();
    
    /// <summary>
    /// Making the constructor private prevents the creation of a new <see cref="Signals.SignalBus"/>
    /// </summary>
    private SignalBus()
    {
    }
    
    /*/// <summary>
    /// T override for Godot's AddUserSignal() that passes the signal's name and NewSignalArgs
    /// </summary>
    /// <typeparam name="TSignal"></typeparam>
    public void AddUserSignal<TSignal>(TSignal signal) where TSignal : UserSignalArgs
    {
        AddUserSignal(signal.SignalName, null);
    }
    
    /// <summary>
    /// T override for Godot's Connect() to make it unnecessary to pass along Signal object,<br />
    /// Use the nameof(method) to get the methodName
    /// </summary>
    /// <param name="node">The node listening for the signal</param>
    /// <param name="methodName">The name of the method from the node</param>
    public Error Connect<TSignal>(TSignal signal, Node node, string methodName, uint flags = 0U) where TSignal : UserSignalArgs
    {
        return Connect(signal.SignalName, new Callable(node, methodName), flags);
    }

    /// <summary>
    /// T override for Godot's EmitSignal that passses the signal's name and SignalArgs
    /// </summary>
    /// <param name="signal"></param>
    /// <typeparam name="TSignal"></typeparam>
    /// <returns></returns>
    public Error EmitSignal<TSignal>(ref TSignal signal) where TSignal : UserSignalArgs
    {
        return EmitSignal(signal.SignalName, signal.SignalArgs);
    }*/
}