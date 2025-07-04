using CS.Components.Damageable;
using CS.SlimeFactory;
using CS.SlimeFactory.Signals;
using Godot;
using Godot.Collections;

namespace CS.Components.CombatManager;

public partial class TurnManagerSystem : NodeSystem
{
    public Node CurrentMobsTurn = new();
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
        _turnOrder.RemoveAt(0);
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
        _turnOrder.Clear();
        _turnOrder = args.players + args.enemies;
        CurrentMobsTurn = _turnOrder[0];
        
        var signal = new StartOfTurnSignal();
        _nodeManager.SignalBus.EmitStartOfTurnSignal(CurrentMobsTurn, ref signal);
    }
}

public partial class StartOfCombatSignal : UserSignalArgs
{
    public Array<Node> players;
    public Array<Node> enemies;

    public StartOfCombatSignal(Array<Node> players, Array<Node> enemies)
    {
        this.players = players;
        this.enemies = enemies;
    }
}

public partial class StartOfTurnSignal : UserSignalArgs
{
    
}

public partial class EndOfTurnSignal : UserSignalArgs
{
    
}