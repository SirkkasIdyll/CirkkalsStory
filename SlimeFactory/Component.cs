using Godot;

namespace CS.SlimeFactory;

public abstract partial class Component : Node2D, IComponent
{
    /// <summary>
    /// Set node name to the type that it is for easier retrieval in <see cref="NodeManager"/>
    /// </summary>
    public override void _Ready()
    {
        base._Ready();
        
        SetName(GetType().Name);
        SetOwner(GetParent());
    }
}