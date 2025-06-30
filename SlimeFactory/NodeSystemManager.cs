using System;
using System.Linq;
using System.Reflection;
using Godot;

namespace CS.SlimeFactory;

public class NodeSystemManager
{
    private NodeSystemManager()
    {
        // Get every NodeSystem and call their _SystemReady function since I can't type-hint Godot's _Ready function
        var enumerable = Assembly
            .GetAssembly(typeof(NodeSystem))
            ?.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(NodeSystem)))
            .GetEnumerator();

        if (enumerable == null)
            return;
        
        while (enumerable.MoveNext())
        {
            GD.Print(enumerable.Current);
            var nodeSystem = (INodeSystem) Activator.CreateInstance(enumerable.Current)!;
            nodeSystem._SystemReady();
        }
    }
    
    /// <summary>
    /// Declare that there can only ever be a single instance of the <see cref="SignalBus"/>
    /// </summary>
    public static NodeSystemManager Instance { get; } = new();
}