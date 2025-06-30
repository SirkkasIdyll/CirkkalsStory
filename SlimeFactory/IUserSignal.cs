namespace CS.SlimeFactory;

/// <summary>
/// I'm implementing user signals because I hate how clunky it is having to use
/// Godot's Signal attribute/EventHandler stuff. It's difficult having to connect each node in the editor and
/// then connecting each node to the exact signals coming from each node.
/// <br />
/// <br />
/// <see cref="SignalBus"/> to the rescue!
/// </summary>
public interface IUserSignal
{
    public string Name { get; set; }
    
    public string GetName()
    {
        return nameof(IUserSignal);
    }
}