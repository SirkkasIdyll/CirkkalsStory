using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace CS.SlimeFactory;

/// <summary>
/// I'm implementing user signals because I hate how clunky it is having to use
/// Godot's Signal attribute/EventHandler stuff. It's difficult having to connect each node in the editor and
/// then connecting each node to the exact signals coming from each node.
/// <br />
/// <br />
/// <see cref="SignalBus"/> to the rescue!
/// </summary>
public abstract partial class UserSignalArgs
{
    public Node Node;
    
    public string SignalName => GetType().Name;

    public Array NewSignalArgs =>
    [
        new Dictionary() 
        { 
            { "name", "Node" }, 
            { "type", (int)Variant.Type.Object }
        }
    ];

    public Array<Variant> SignalArgs = new();

    protected UserSignalArgs(Node node)
    {
        Node = node;
        SignalArgs.Add(node);
    }
}