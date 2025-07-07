using CS.Components.Damageable;
using CS.SlimeFactory;
using CS.SlimeFactory.Signals;
using Godot;
using Godot.Collections;

namespace CS.Components.CombatManager;

public partial class TurnManagerSystem : NodeSystem
{
    public Node? CurrentMobsTurn;
    private Array<Node> _turnOrder = [];
	
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();

        _nodeManager.SignalBus.EndOfTurnSignal += OnEndOfTurnSignal;
        _nodeManager.SignalBus.MobDiedSignal += OnMobDied;
        _nodeManager.SignalBus.StartOfCombatSignal += OnStartOfCombat;
    }
    
    /// <summary>
    /// When one mob's turn ends, send them to the end of the turn order
    /// Then signal the start of the next mob's turn
    /// </summary>
    /// <param name="node"></param>
    /// <param name="args"></param>
    private void OnEndOfTurnSignal(Node node, ref EndOfTurnSignal args)
    {
        // Only re-add mobs that are in the turn order, those that aren't were dead and removed
        if (_turnOrder.Remove(node))
            _turnOrder.Add(node);
        
        CurrentMobsTurn = _turnOrder[0];
        var signal = new StartOfTurnSignal();
        _nodeManager.SignalBus.EmitStartOfTurnSignal(CurrentMobsTurn, ref signal);
    }

    private void OnMobDied(Node node, ref MobDiedSignal args)
    {
        _turnOrder.Remove(node);
    }

    /// <summary>
    /// Set the turn order for combat and signal the start of the first mob's turn
    /// TODO: Make turn order calculations more complicated
    /// </summary>
    /// <param name="node"></param>
    /// <param name="args"></param>
    private void OnStartOfCombat(Node node, ref StartOfCombatSignal args)
    {
        _turnOrder = args.Players + args.Enemies;
        CurrentMobsTurn = _turnOrder[0];
        
        var signal = new StartOfTurnSignal();
        _nodeManager.SignalBus.EmitStartOfTurnSignal(CurrentMobsTurn, ref signal);
    }
}

public partial class StartOfCombatSignal : UserSignalArgs
{
    public Array<Node> Players;
    public Array<Node> Enemies;

    public StartOfCombatSignal(Array<Node> players, Array<Node> enemies)
    {
        Players = players;
        Enemies = enemies;
    }
}

public partial class EndOfCombatSignal : UserSignalArgs
{
    public bool Won;
}

public partial class StartOfTurnSignal : UserSignalArgs
{
}

public partial class EndOfTurnSignal : UserSignalArgs
{
}

public partial class EnemyTurnSignal : UserSignalArgs
{
    public Array<Node> Players;
    public Array<Node> Enemies;

    public EnemyTurnSignal(Array<Node> players, Array<Node> enemies)
    {
        Players = players;
        Enemies = enemies;
    }
}