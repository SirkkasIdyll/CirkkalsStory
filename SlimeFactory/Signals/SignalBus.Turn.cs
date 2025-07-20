using CS.Components.CombatManager;
using CS.Components.Mob;

namespace CS.SlimeFactory.Signals;

public partial class SignalBus
{
    public delegate void StartOfCombatSignalHandler(ref StartOfCombatSignal args);
    public event StartOfCombatSignalHandler? StartOfCombatSignal;
    public void EmitStartOfCombatSignal(ref StartOfCombatSignal args)
    {
        StartOfCombatSignal?.Invoke(ref args);
    }
    
    public delegate void EndOfCombatSignalHandler(ref EndOfCombatSignal args);
    public event EndOfCombatSignalHandler? EndOfCombatSignal;
    public void EmitEndOfCombatSignal(ref EndOfCombatSignal args)
    {
        EndOfCombatSignal?.Invoke(ref args);
    }

    public delegate void GameOverSignalHandler();
    public event GameOverSignalHandler? GameOverSignal;
    public void EmitGameOverSignal()
    {
        GameOverSignal?.Invoke();
    }
    
    public delegate void StartOfTurnSignalHandler(Node<MobComponent> node, ref StartOfTurnSignal args);
    public event StartOfTurnSignalHandler? StartOfTurnSignal;
    public void EmitStartOfTurnSignal(Node<MobComponent> node, ref StartOfTurnSignal args)
    {
        StartOfTurnSignal?.Invoke(node, ref args);
    }

    public delegate void EndOfTurnSignalHandler(Node<MobComponent> node, ref EndOfTurnSignal args);
    public event EndOfTurnSignalHandler? EndOfTurnSignal;
    public void EmitEndOfTurnSignal(Node<MobComponent> node, ref EndOfTurnSignal args)
    {
        EndOfTurnSignal?.Invoke(node, ref args);
    }

    public delegate void EnemyTurnSignalHandler(Node<MobComponent> node, ref EnemyTurnSignal args);
    public event EnemyTurnSignalHandler? EnemyTurnSignal;
    public void EmitEnemyTurnSignal(Node<MobComponent> node, ref EnemyTurnSignal args)
    {
        EnemyTurnSignal?.Invoke(node, ref args);
    }

    public delegate void UseActionSignalHandler(Node<MobComponent> node, ref UseActionSignal args);
    public event UseActionSignalHandler? UseActionSignal;
    public void EmitUseActionSignal(Node<MobComponent> node, ref UseActionSignal args)
    {
        UseActionSignal?.Invoke(node, ref args);
    }

    public delegate void PreviewActionSignalHandler(Node<MobComponent> node, ref PreviewActionSignal args);
    public event PreviewActionSignalHandler? PreviewActionSignal;
    public void EmitPreviewActionSignal(Node<MobComponent> node, ref PreviewActionSignal args)
    {
        PreviewActionSignal?.Invoke(node, ref args);
    }
}