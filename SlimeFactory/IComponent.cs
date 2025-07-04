using Godot;

namespace CS.SlimeFactory;

public interface IComponent
{
    public Node ParentNode { get; }
}