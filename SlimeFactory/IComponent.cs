using Godot;

namespace PC.SlimeFactory;

public interface IComponent
{
    public Node Owner { get; }

    // public string Serialize();
}