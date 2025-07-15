using System;
using System.Linq;
using System.Reflection;
using Godot;

namespace CS.SlimeFactory;

/// <summary>
/// All node systems are added to the main scene by the <see cref="NodeSystemManager"/>
/// All node systems have access to the <see cref="NodeManager"/> which itself has access to the <see cref="Signals.SignalBus"/>
/// </summary>
public abstract partial class NodeSystem : Node2D, INodeSystem
{
    protected readonly NodeManager _nodeManager = NodeManager.Instance;
    protected readonly NodeSystemManager _nodeSystemManager = NodeSystemManager.Instance;
    
    /// <summary>
    /// Adds every constructed <see cref="NodeSystem"/> to the given mainScene when called by the <see cref="NodeSystemManager"/>
    /// The reason we add it to the mainScene is so that it has access to the SceneTree,
    /// which enables functions like Ready() or Process()
    /// </summary>
    /// <param name="mainScene">The root scene for the project</param>
    public void AddToMainScene(Node mainScene)
    {
        mainScene.AddChild(this);
    }

    /// <summary>
    /// After all systems are initialized, system dependencies can be injected without worry of order of initialization
    /// </summary>
    public void _InjectDependencies()
    {
        var fields = GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(t => t.GetCustomAttribute<InjectDependency>(false) != null);
        foreach (var field in fields)
        {
            if (!_nodeSystemManager.NodeSystemDictionary.TryGetValue(field.FieldType.Name, out var nodeSystem))
                continue;
            
            field.SetValue(this, nodeSystem);
            GD.Print("Injected " + nodeSystem.Name + " as a dependency");
        }
    }

    /// <summary>
    /// Set node name to the type that it is for easier retrieval in <see cref="NodeSystemManager"/>
    /// </summary>
    public override void _Ready()
    {
        base._Ready();
        
        SetName(GetType().Name);
        SetOwner(GetParent());
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class InjectDependency : Attribute { }