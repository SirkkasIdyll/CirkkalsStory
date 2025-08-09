using System;
using System.Collections.Generic;
using CS.Resources.Encounters;
using CS.Resources.Events;
using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Components.CombatManager;

public partial class ScenarioSystem : NodeSystem
{
    private const string EncountersPath = "res://Resources/Encounters/";
    private const string EventsPath = "res://Resources/Events/";
    private List<StoryEvent> _storyEvents = [];
    private int _stage = 2;
    private int _counter = 1;
    
    public Array<String> PreviousEnemies = [];
    
    public override void _Ready()
    {
        base._Ready();
        
        LoadEvents();

        _nodeManager.SignalBus.EndOfCombatSignal += OnEndOfCombat;
        _nodeManager.SignalBus.GameOverSignal += OnGameOverSignal;
    }

    private void OnEndOfCombat(ref EndOfCombatSignal args)
    {
        if (!args.Won)
            return;

        var storyEvent = GetEventBasedOnCurrentProgress();
        if (storyEvent == null)
            return;
        
        var signal = new ChangeActiveSceneSignal(storyEvent.PackedScene);
        _nodeManager.SignalBus.EmitChangeActiveSceneSignal(args.CombatScene, ref signal);
    }

    private void OnGameOverSignal()
    {
        _stage = 1;
        _counter = 1;
    }

    /// <summary>
    /// Get all events in the events folder
    /// </summary>
    private void LoadEvents()
    {
        // The possible mob resources to spawn
        var files = _nodeManager.GetFilesByExtension(EventsPath, ".tres");

        foreach (var file in files)
        {
            var storyEvent = ResourceLoader.Load<StoryEvent>(file);
            _storyEvents.Add(storyEvent);
        }
    }
    
    /// <summary>
    /// Picks an event based on current stage and counter
    /// </summary>
    private StoryEvent? GetEventBasedOnCurrentProgress()
    {
        // Create a list of valid events to choose from based on all events in the game
        // based on current stage and counter
        List<StoryEvent> validEvents = [];
        foreach (var storyEvent in _storyEvents)
        {
            if (storyEvent.Stage != _stage && storyEvent.Stage != 0)
                continue;

            if (storyEvent.Counter != _counter && storyEvent.Counter != 0)
                continue;

            if (storyEvent.Mandatory)
                return storyEvent;
            
            validEvents.Add(storyEvent);
        }

        // We just simply don't have anything left
        if (validEvents.Count == 0)
            return null;
        
        // Pick an event based on the pick weight of the events
        // and remove the event from further choosing if the event is unique
        var weights = new float[validEvents.Count];
        for (var i = 0; i < validEvents.Count; i++)
            weights[i] = validEvents[i].PickWeight;
        var rng = new RandomNumberGenerator();
        var index = (int)rng.RandWeighted(weights);
        var chosenEvent = validEvents[index];
        if (chosenEvent.Unique)
            _storyEvents.Remove(chosenEvent);
        return chosenEvent;
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
        var path = EncountersPath + _stage + "-";
        if (_counter <= 3)
            path += "Basic/";
        else
            path += "Boss/";
        using var dir = DirAccess.Open(path);
        
        // The possible mob resources to spawn
        var files = dir.GetFiles();
        
        // Choose a random resource to spawn
        var random = files[Random.Shared.Next(0, files.Length)];
        var combatScenarioResource = ResourceLoader.Load<CombatEncounterResource>(path + random);
        
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