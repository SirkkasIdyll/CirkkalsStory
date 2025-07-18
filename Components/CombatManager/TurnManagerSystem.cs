using CS.Components.Damageable;
using CS.Components.Description;
using CS.Components.Mob;
using CS.SlimeFactory;
using CS.SlimeFactory.Signals;
using Godot;
using Godot.Collections;

namespace CS.Components.CombatManager;

public partial class TurnManagerSystem : NodeSystem
{
    [InjectDependency] private readonly DescriptionSystem _descriptionSystem = default!;
    
    private Array<Node> _turnOrder = [];
    private Array<Node> _deadMobs = [];
    public Array<Node> Players = [];
    public Array<Node> Enemies = [];
	
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();

        _nodeManager.SignalBus.MobDiedSignal += OnMobDied;
    }

    public Node GetActiveMob()
    {
        return _turnOrder[0];
    }

    public StartOfCombatSignal? StartCombat()
    {
        if (Players.Count <= 0)
            return null;

        if (Enemies.Count <= 0)
            return null;
        
        _turnOrder = Players + Enemies;
        var signal = new StartOfCombatSignal(Players, Enemies);
        _nodeManager.SignalBus.EmitStartOfCombatSignal(ref signal);
        return signal;
    }

    public StartOfTurnSignal? StartTurn()
    {
        // GD.Print("Start of " + _descriptionSystem.GetDisplayName(GetActiveMob()) + "'s turn");
        if (!_nodeManager.TryGetComponent<MobComponent>(GetActiveMob(), out var mobComponent))
            return null;
        
        var signal = new StartOfTurnSignal();
        _nodeManager.SignalBus.EmitStartOfTurnSignal((GetActiveMob(), mobComponent), ref signal);
        return signal;
    }

    public EndOfTurnSignal? EndTurn()
    {
        // GD.Print("End of " + _descriptionSystem.GetDisplayName(GetActiveMob()) + "'s turn");
        if (!_nodeManager.TryGetComponent<MobComponent>(GetActiveMob(), out var mobComponent))
            return null;

        var temp = GetActiveMob();
        _turnOrder.Add(GetActiveMob());
        _turnOrder.Remove(GetActiveMob());
        
        var signal = new EndOfTurnSignal();
        _nodeManager.SignalBus.EmitEndOfTurnSignal((temp, mobComponent), ref signal);
        
        return signal;
    }

    public EndOfCombatSignal EndCombat(Node combatScene)
    {
        var signal = Enemies.Count == 0
            ? new EndOfCombatSignal(combatScene, true)
            : new EndOfCombatSignal(combatScene, false);
        _nodeManager.SignalBus.EmitEndOfCombatSignal(ref signal);
        
        _turnOrder.Clear();
        Players.Clear();
        Enemies.Clear();
        return signal;
    }
    
    private void OnMobDied(Node node, ref MobDiedSignal args)
    {
        _turnOrder.Remove(node);
        Players.Remove(node);
        Enemies.Remove(node);
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
    public Node CombatScene;
    public bool Won;

    public EndOfCombatSignal(Node combatScene, bool won)
    {
        CombatScene = combatScene;
        Won = won;
    }
}

public partial class StartOfTurnSignal : UserSignalArgs
{
    public Array<string> Summaries = [];
}

public partial class EndOfTurnSignal : UserSignalArgs
{
    public Array<string> Summaries = [];
}

public partial class EnemyTurnSignal : CancellableSignalArgs
{
    public Array<Node> Players;
    public Array<Node> Enemies;
    public Array<string> Summaries = [];

    public EnemyTurnSignal(Array<Node> players, Array<Node> enemies)
    {
        Players = players;
        Enemies = enemies;
    }
}

public partial class UseActionSignal : UserSignalArgs
{
    public Node Action;
    public Array<Node> Targets;
    public Array<string> Summaries = [];

    public UseActionSignal(Node action, Array<Node> targets)
    {
        Action = action;
        Targets = targets;
    }
}