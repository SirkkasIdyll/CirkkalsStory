using System;
using CS.Resources.CombatScenarios;
using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Components.CombatManager;

public partial class CombatScenarioSystem : NodeSystem
{
    private const string CombatScenarioPath = "res://Resources/CombatScenarios/";
    private int _stage = 1;
    private int _counter = 1;
    
    public override void _Ready()
    {
        base._Ready();

        _nodeManager.SignalBus.GameOverSignal += OnGameOverSignal;
    }

    private void OnGameOverSignal()
    {
        _stage = 1;
        _counter = 1;
    }

    public Array<Node> GetNextEnemies()
    {
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
			 
            enemies.Add(mobNode);
        }
        
        _counter++;
        return enemies;
    }
}