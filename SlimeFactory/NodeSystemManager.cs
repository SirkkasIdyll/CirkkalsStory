using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Godot;

namespace CS.SlimeFactory;

/// <summary>
/// Responsible for adding <see cref="NodeSystem"/> to the mainScene and readying them
/// If you need access to this, go through <see cref="NodeSystem"/>'s instance of it
/// </summary>
public class NodeSystemManager
{
    /// <summary>
    /// Declare that there can only ever be a single instance of the <see cref="Signals.SignalBus"/>
    /// </summary>
    public static NodeSystemManager Instance { get; } = new();
    private Node? _mainScene;
    
    private NodeSystemManager()
    {
    }
    
    /// <summary>
    /// Gets every <see cref="NodeSystem"/> and calls their _SystemReady functions, which initializes them,
    /// the <see cref="NodeManager"/>, and the <see cref="Signals.SignalBus"/>
    /// </summary>
    public void InitializeNodeSystems(Node mainScene)
    {
        _mainScene = mainScene;
        
        // Get every NodeSystem and call their _SystemReady function since I can't type-hint Godot's _Ready function
        var nodeSystemEnumerator = Assembly
            .GetAssembly(typeof(NodeSystem))
            ?.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(NodeSystem)))
            .GetEnumerator();

        if (nodeSystemEnumerator == null)
            return;
        
        while (nodeSystemEnumerator.MoveNext())
        {
            // GD.Print(nodeSystemEnumerator.Current);
            var nodeSystem = (INodeSystem) Activator.CreateInstance(nodeSystemEnumerator.Current)!;
            // nodeSystem._SystemReady();
            nodeSystem.AddToMainScene(mainScene);
        }
    }

    /// <summary>
    /// Every system should be a child of the main scene, so we'll check the main scene
    /// to see if the node system is there.
    /// </summary>
    /// <param name="nodeSystem"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool TryGetNodeSystem<T>([NotNullWhen(true)] out T? nodeSystem) where T : class, INodeSystem
    {
        nodeSystem = default;
        if (_mainScene == null)
            return false;

        nodeSystem = _mainScene.GetNodeOrNull<T>($"{typeof(T).Name}");
        return nodeSystem != null;
    }
}