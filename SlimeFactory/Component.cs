using Godot;

namespace CS.SlimeFactory;

public partial class Component : Node2D, IComponent
{
    public Node ParentNode => GetParent();
    
    /// <summary>
    /// Set node name to the type that it is for easier retrieval in <see cref="NodeManager"/>
    /// </summary>
    public override void _Ready()
    {
        base._Ready();
        
        SetName(GetType().Name);
    }
}