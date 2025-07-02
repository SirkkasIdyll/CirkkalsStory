using Godot;

namespace CS.SlimeFactory;

public partial class Component : Node2D, IComponent
{
    public string CompName => GetType().Name;

    public Node ParentNode => GetParent();
}