using CS.Components.Grid;
using Godot;

namespace CS.SlimeFactory.Signals;

public partial class SignalBus
{
    public delegate void NodeAnchoredToGridSignalHandler(RigidBody2D node, ref NodeAnchoredToGridSignal args);
    public event NodeAnchoredToGridSignalHandler? NodeAnchoredToGridSignal;
    public void EmitNodeAnchoredToGridSignal(RigidBody2D node, ref NodeAnchoredToGridSignal args)
    {
        NodeAnchoredToGridSignal?.Invoke(node, ref args);
    }

    public delegate void NodeUnanchoredFromGridSignalHandler(RigidBody2D node, ref NodeUnanchoredFromGridSignal args);
    public event NodeUnanchoredFromGridSignalHandler? NodeUnanchoredFromGridSignal;
    public void EmitNodeUnanchoredFromGridSignal(RigidBody2D node, ref NodeUnanchoredFromGridSignal args)
    {
        NodeUnanchoredFromGridSignal?.Invoke(node, ref args);
    }
}