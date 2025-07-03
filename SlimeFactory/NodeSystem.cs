using System;
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
    
    public virtual void _SystemReady()
    {
    }
}