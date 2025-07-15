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
    }

    public Array<Node> GetNextEnemies()
    {
        Array<Node> enemies = [];
        if (_counter >= 5)
        {
            _counter = 1;
            _stage++;
        }
        var path = CombatScenarioPath + _stage + "-";
        if (_counter <= 3)
            path += "Basic/";
        else
            path += "Boss/";
        using var dir = DirAccess.Open(path);
        var files = dir.GetFiles();
        var random = files[Random.Shared.Next(0, files.Length)];
        var combatScenarioResource = ResourceLoader.Load<CombatScenarioResource>(path + random);
        var mobs = combatScenarioResource.Mobs;
        foreach (var mob in mobs)
        {
            if (!_nodeManager.TryInstantiateNode(mob, out var mobNode))
                continue;
			 
            enemies.Add(mobNode);
        }

        _counter++;
        return enemies;
    }
}