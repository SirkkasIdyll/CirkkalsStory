using CS.Components.CombatManager;
using CS.Components.Mob;
using Godot;

namespace CS.SlimeFactory.Signals;

public partial class SignalBus
{
    public delegate void StartOfCombatSignalHandler(Node node, ref StartOfCombatSignal args);
    public event StartOfCombatSignalHandler StartOfCombatSignal;
    public void EmitStartOfCombatSignal(Node node, ref StartOfCombatSignal args)
    {
        StartOfCombatSignal?.Invoke(node, ref args);
    }
    
    public delegate void StartOfTurnSignalHandler(Node node, ref StartOfTurnSignal args);
    public event StartOfTurnSignalHandler? StartOfTurnSignal;
    public void EmitStartOfTurnSignal(Node node, ref StartOfTurnSignal args)
    {
        StartOfTurnSignal?.Invoke(node, ref args);
    }

    public delegate void EndOfTurnSignalHandler(Node node, ref EndOfTurnSignal args);
    public event EndOfTurnSignalHandler? EndOfTurnSignal;
    public void EmitEndOfTurnSignal(Node node, ref EndOfTurnSignal args)
    {
        EndOfTurnSignal?.Invoke(node, ref args);
    }
}