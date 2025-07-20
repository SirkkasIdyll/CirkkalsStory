using System;
using CS.Resources.CombatScenarios;
using CS.Resources.RandomScenarios;
using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Components.CombatManager;

public partial class ScenarioSystem : NodeSystem
{
    private const string CombatScenarioPath = "res://Resources/CombatScenarios/";
    private const string RandomScenarioPath = "res://Resources/RandomScenarios/";
    private int _stage = 1;
    private int _counter = 1;
    
    public Array<String> PreviousEnemies = [];
    
    public override void _Ready()
    {
        base._Ready();

        _nodeManager.SignalBus.EndOfCombatSignal += OnEndOfCombat;
        _nodeManager.SignalBus.GameOverSignal += OnGameOverSignal;
    }

    private void OnEndOfCombat(ref EndOfCombatSignal args)
    {
        if (!args.Won)
            return;
        
        // Example file path: res://Resources/RandomScenarios/1-Basic/
        var path = RandomScenarioPath + _stage + "-";
        if (_counter <= 3)
            path += "Basic/";
        else
            path += "Boss/";
        using var dir = DirAccess.Open(path);
        
        // The possible mob resources to spawn
        var files = dir.GetFiles();
        
        // Choose a random resource to spawn
        var random = files[Random.Shared.Next(0, files.Length)];
        var randomScenarioResource = ResourceLoader.Load<RandomScenarioResource>(path + random);
        
        var signal = new ChangeActiveSceneSignal(randomScenarioResource.PackedScene!);
        _nodeManager.SignalBus.EmitChangeActiveSceneSignal(args.CombatScene, ref signal);
    }

    private void OnGameOverSignal()
    {
        _stage = 1;
        _counter = 1;
    }

    public Array<Node> GetNextEnemies()
    {
        PreviousEnemies.Clear();
        Array<Node> enemies = [];
        // Counter 1-3 = Basic Enemy
        // Counter 4 =  Boss Enemy
        // Counter 5 and up = Next stage, back to basic enemies
        if (_counter >= 5)
        {
            _counter = 1;
            _stage++;
        }
        
        // Example file path: res://Resources/CombatScenarios/1-Basic/
        var path = CombatScenarioPath + _stage + "-";
        if (_counter <= 3)
            path += "Basic/";
        else
            path += "Boss/";
        using var dir = DirAccess.Open(path);
        
        // The possible mob resources to spawn
        var files = dir.GetFiles();
        
        // Choose a random resource to spawn
        var random = files[Random.Shared.Next(0, files.Length)];
        var combatScenarioResource = ResourceLoader.Load<CombatScenarioResource>(path + random);
        
        // Try spawning the mobs listed in the resource and add them to the result
        var mobs = combatScenarioResource.Mobs;
        foreach (var mob in mobs)
        {
            if (!_nodeManager.TrySpawnNode(mob, out var mobNode))
                continue;
            
            PreviousEnemies.Add(mobNode.Name);
            enemies.Add(mobNode);
        }
        
        _counter++;
        return enemies;
    }
    
    
}