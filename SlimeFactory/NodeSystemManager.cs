using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Godot;

namespace CS.SlimeFactory;

public class NodeSystemManager
{
    private Node? _mainScene;
    
    private NodeSystemManager()
    {
    }
    
    /// <summary>
    /// Declare that there can only ever be a single instance of the <see cref="SignalBus"/>
    /// </summary>
    public static NodeSystemManager Instance { get; } = new();

    /// <summary>
    /// Gets every <see cref="NodeSystem"/> and calls their _SystemReady functions, which initializes them,
    /// the <see cref="NodeManager"/>, and the <see cref="SignalBus"/>
    /// </summary>
    public void Initialize(Node mainScene)
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
            nodeSystem._SystemReady();
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
    public bool TryGetNodeSystem<T>([NotNullWhen(true)] out T? nodeSystem) where T : INodeSystem
    {
        nodeSystem = default;
        if (_mainScene == null)
            return false;
        
        var children = _mainScene.GetChildren();
        
        foreach (var child in children)
        {
            if (child is not T generic)
                continue;
            
            nodeSystem = generic;
            return true;
        }

        return false;
    }
}