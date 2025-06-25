using System.Diagnostics.CodeAnalysis;
using Godot;

namespace CS.Components;

public static partial class ComponentSystem
{
    /// <summary>
    /// Goes through each child of the node and checks if the child is of the same type as T.
    /// </summary>
    /// <param name="node">Mob, skill, whatever</param>
    /// <returns>True if component was found, false if otherwise</returns>
    public static bool HasComponent<T>(Node node)
    {
        var children = node.GetChildren();

        foreach (var child in children)
        {
            if (child is T)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Goes through each child of the node and returns the child with the type of T
    /// </summary>
    /// <param name="node">Mob, skill, whatever</param>
    /// <param name="component">The component if found, null otherwise</param>
    /// <returns>True if component was found, false if otherwise</returns>
    public static bool TryGetComponent<T>(Node node, [NotNullWhen(true)] out T? component)
    {
        var children = node.GetChildren();
        
        foreach (var child in children)
        {
            if (child is not T generic)
                continue;
            
            component = generic;
            return true;
        }

        component = default;
        return false;
    }
}