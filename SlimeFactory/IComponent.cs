using Godot;

namespace CS.SlimeFactory;

public interface IComponent
{
    public string CompName { get;}
    
    public Node ParentNode { get; }
}