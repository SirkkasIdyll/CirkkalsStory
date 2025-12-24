using CS.SlimeFactory;
using Godot;

namespace CS.Nodes.Scenes;

/// <summary>
/// If a UI scene needs to have its details modified,
/// add this interface to the scene system to require the method
/// </summary>
public interface IModifiableScene
{
    /// <summary>
    /// UI owner and component should be assigned,
    /// title and specific details of the UI should be set
    /// </summary>
    /// <param name="node"></param>
    public void SetDetails(Node node);
}