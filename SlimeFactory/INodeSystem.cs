using Godot;

namespace PC.SlimeFactory;

/// <summary>
/// This exists so we can type-hint it inside <see cref="NodeSystemManager"/>
/// </summary>
public interface INodeSystem
{
    public void AddToMainScene(Node mainScene) { }
    
    public virtual void _InjectDependencies() { }
}