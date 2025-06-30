using Godot;

namespace CS.SlimeFactory;

/// <summary>
/// All node systems have access to the <see cref="NodeManager"/> which itself has access to the <see cref="SignalBus"/>
/// </summary>
public abstract partial class NodeSystem : Node2D, INodeSystem
{
    public virtual void _SystemReady()
    {
    }
    
    protected NodeManager NodeManager = NodeManager.Instance;
    protected NodeSystemManager NodeSystemManager = NodeSystemManager.Instance;
    protected SignalBus SignalBus = SignalBus.Instance;
}