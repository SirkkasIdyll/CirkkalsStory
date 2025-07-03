using Godot;

namespace CS.SlimeFactory;

/// <summary>
/// This exists so we can type-hint it inside <see cref="NodeSystemManager"/>
/// </summary>
public interface INodeSystem
{
    public void AddToMainScene(Node mainScene) { }

    /// <summary>
    /// Called once by the <see cref="NodeSystemManager"/> upon its construction<br />
    /// The custom NodeSystem equivalent of the _Ready function
    /// </summary>
    public void _SystemReady() { }
}