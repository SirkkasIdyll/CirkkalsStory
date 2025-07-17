namespace CS.SlimeFactory.Signals;

/// <summary>
/// I'm implementing user signals because I hate how clunky it is having to use
/// Godot's Signal attribute/EventHandler stuff. It's difficult having to connect each node in the editor and
/// then connecting each node to the exact signals coming from each node.
/// <br />
/// <br />
/// <see cref="Signals.SignalBus"/> to the rescue!
/// </summary>
public abstract partial class UserSignalArgs
{
    // public string SignalName => GetType().Name;
}

public abstract partial class HandledSignalArgs : UserSignalArgs
{
    /// <summary>
    /// If the signal is marked as handled, don't process the signal by any other systems
    /// </summary>
    public bool Handled;
}

public abstract partial class CancellableSignalArgs : UserSignalArgs
{
    /// <summary>
    /// If the signal is marked as Canceled, don't process the signal by any other systems
    /// </summary>
    public bool Canceled;
}