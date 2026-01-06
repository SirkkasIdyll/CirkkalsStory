using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Godot;
using Godot.Collections;

namespace PC.SlimeFactory;

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

    public readonly Dictionary<string, NodeSystem> NodeSystemDictionary = [];
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
        NodeSystemDictionary.Clear();
        _mainScene = mainScene;

        var nodeSystemEnumerator = Assembly
            .GetAssembly(typeof(NodeSystem))
            ?.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(NodeSystem)))
            .GetEnumerator();

        if (nodeSystemEnumerator == null)
            return;

        while (nodeSystemEnumerator.MoveNext())
        {
            var nodeSystem = (NodeSystem)Activator.CreateInstance(nodeSystemEnumerator.Current)!;
            nodeSystem.AddToMainScene(mainScene);
            NodeSystemDictionary.Add(nodeSystem.Name, nodeSystem);
        }
    }

    /// <summary>
    /// After all systems are initialized, system dependencies can be injected without worry of order of initialization
    /// </summary>
    public void InjectNodeSystemDependencies()
    {
        foreach (var nodeSystem in NodeSystemDictionary.Values)
        {
            var fields = nodeSystem.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(t => t.GetCustomAttribute<InjectDependency>(false) != null);
            foreach (var field in fields)
            {
                if (NodeSystemDictionary.TryGetValue(field.FieldType.Name, out var dependency))
                    field.SetValue(nodeSystem, dependency);

                if (field.FieldType.Name == NodeManager.Instance.GetType().Name)
                    field.SetValue(nodeSystem, NodeManager.Instance);

                if (field.FieldType.Name == NodeSystemManager.Instance.GetType().Name)
                    field.SetValue(nodeSystem, NodeSystemManager.Instance);
                // GD.Print("Injected " + nodeSystem.Name + " as a dependency");
            }
        }
    }

    /// <summary>
    /// Attempt to inject node system dependencies after the fact, usually for UI systems
    /// </summary>
    public void InjectNodeSystemDependencies(Control uiSystem)
    {
        var fields = uiSystem.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(t => t.GetCustomAttribute<InjectDependency>(false) != null);
        foreach (var field in fields)
        {
            if (NodeSystemDictionary.TryGetValue(field.FieldType.Name, out var dependency))
                field.SetValue(uiSystem, dependency);

            if (field.FieldType.Name == NodeManager.Instance.GetType().Name)
                field.SetValue(uiSystem, NodeManager.Instance);

            if (field.FieldType.Name == NodeSystemManager.Instance.GetType().Name)
                field.SetValue(uiSystem, NodeSystemManager.Instance);
            // GD.Print("Injected " + nodeSystem.Name + " as a dependency");
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

/// <summary>
/// <see cref="NodeSystemManager"/> will run the _InjectDepencies() function on each NodeSystem
/// Each <see cref="NodeSystem"/> will go through its PRIVATE field instances with this attribute
/// and assign the appropriate system to it
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class InjectDependency : Attribute { }