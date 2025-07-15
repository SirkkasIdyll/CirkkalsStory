using Godot;

namespace CS.SlimeFactory;

public interface IComponent
{
    public Node Owner { get; }

    // public string Serialize();
}