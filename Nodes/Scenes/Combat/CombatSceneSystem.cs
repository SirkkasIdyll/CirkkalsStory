using System;
using System.Collections.Generic;
using CS.Components.CombatManager;
using CS.Components.Damageable;
using CS.Components.Mob;
using CS.Components.Skills;
using CS.Nodes.Scenes.Combat.SkillSelection;
using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Nodes.Scenes.Combat;

/// <summary>
/// 1. Load mobs and actions in
/// 2. On specific actions pressed, listen to options
/// 3. Once ability chosen, choose target
/// 4. Once target chosen, apply combat effects to the target
/// </summary>
public partial class CombatSceneSystem : Control
{
	private readonly NodeManager _nodeManager = NodeManager.Instance;
	private readonly NodeSystemManager _nodeSystemManager = NodeSystemManager.Instance;
	private SkillManagerSystem _skillManagerSystem = default!;
	private TurnManagerSystem _turnManagerSystem = default!;
	
	private Node? _chosenSkill;
	
	[ExportCategory("Instantiated")]
	[Export] private Array<Node> _players = [];
	[Export] private Array<Node> _enemies = [];

	[ExportCategory("Owned")]
	[Export] private PackedScene _combatMobRepresentation = default!;
	[Export] private PackedScene _combatSkillSelection = default!;
	[Export] private VBoxContainer _playersVBoxContainer = default!;
	[Export] private VBoxContainer _enemiesVBoxContainer = default!;
	[Export] private ActionsItemListSystem _actionsItemList = default!;
	
	/// <summary>
	/// Once the targets are chosen, we enact the skill on those targets
	/// TODO: Skill impact needs more fleshing out, such as lifesteal or damage resistance
	/// </summary>
	[Signal]
	public delegate void SkillTargetChosenEventHandler(Node attacker, Node defender);
	
	/// <summary>
	/// We display a textual summary in a UI element of what happened
	/// Example: Slime attacked wolf!
	/// </summary>
	[Signal]
	public delegate void DisplayActionSummaryEventHandler(string summary);
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_nodeManager.SignalBus.MobDiedSignal += OnMobDied;
		_nodeManager.SignalBus.StartOfTurnSignal += OnStartOfTurn;
		
		LoadHealthBars(_playersVBoxContainer, _players);
		LoadHealthBars(_enemiesVBoxContainer, _enemies);
		_actionsItemList.SkillsPressed += OnSkillsPressed;
		_actionsItemList.FleePressed += OnFleePressed;
		_actionsItemList.SetVisible(false);
		
		if (_nodeSystemManager.TryGetNodeSystem<SkillManagerSystem>(out var skillManagerSystem))
			_skillManagerSystem = skillManagerSystem;

		GD.Print(this);
		if (_nodeSystemManager.TryGetNodeSystem<TurnManagerSystem>(out var turnManagerSystem))
		{
			_turnManagerSystem = turnManagerSystem;

			var signal = new StartOfCombatSignal(_players, _enemies);
			_nodeManager.SignalBus.EmitStartOfCombatSignal(this, ref signal);
		}
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		_nodeManager.SignalBus.MobDiedSignal -= OnMobDied;
		_nodeManager.SignalBus.StartOfTurnSignal -= OnStartOfTurn;
	}

	/// <summary>
	/// Closes the skill list and opens up the actions list
	/// </summary>
	/// <param name="node"></param>
	private void OnEscapePressed(Node node)
	{
		node.QueueFree();
		_actionsItemList.SetVisible(true);
	}

	private void OnFleePressed()
	{
		var signal = new EndOfCombatSignal();
		_nodeManager.SignalBus.EmitEndOfCombatSignal(this, ref signal);
	}

	/// <summary>
	/// On death remove mob from players or enemies
	/// as well as turn order
	/// </summary>
	private void OnMobDied(Node node, ref MobDiedSignal args)
	{
        _players.Remove(node);
        _enemies.Remove(node);

        var signal = new EndOfCombatSignal();
        if (_enemies.Count == 0)
	        signal.Won = true;
        
        if (_players.Count == 0 || _enemies.Count == 0)
        {
	        _nodeManager.SignalBus.EmitEndOfCombatSignal(this, ref signal);
        }
	}

	/// <summary>
	/// On the players turn, make their actions visible again
	/// On the enemies turn, make them choose a random action
	/// </summary>
	/// <param name="node">The mob whose turn it is</param>
	/// <param name="args"></param>
	private void OnStartOfTurn(Node node, ref StartOfTurnSignal args)
	{
		// If either side has no one left, don't let anyone take their turn to prevent infinite loops
		if (_players.Count == 0 || _enemies.Count == 0)
			return;
		
		if (_players.Contains(node))
		{
			_actionsItemList.SetVisible(true);
			return;
		}

		if (!_nodeManager.TryGetComponent<MobComponent>(node, out var mobComponent))
			return;
		
		var enemyTurnSignal = new EnemyTurnSignal(_players, _enemies);
		_nodeManager.SignalBus.EmitEnemyTurnSignal((node, mobComponent), ref enemyTurnSignal);
		
		var endOfTurnSignal = new EndOfTurnSignal();
		_nodeManager.SignalBus.EmitEndOfTurnSignal(node, ref endOfTurnSignal);
	}

	/// <summary>
	/// Show off the skill selection scene when skill option is selected
	/// </summary>
	private void OnSkillsPressed()
	{
		_actionsItemList.SetVisible(false);

		if (!_nodeManager.TryGetComponent<MobComponent>(_players[0], out var mobComponent))
			return;
		
		var node = _combatSkillSelection.Instantiate<CombatSkillSelectionSceneSystem>();
		node.EscapePressed += OnEscapePressed;
		AddChild(node);
		node.SetSkills(mobComponent.Skills);
		node.SkillChosen += OnSkillChosen;
	}

	/// <summary>
	/// When a skill is chosen from the list,
	/// show off targets based on the skill's <see cref="TargetingComponent"/> setting
	/// </summary>
	/// <param name="skill"></param>
	private void OnSkillChosen(string skill)
	{
		if (!_skillManagerSystem.TryGetSkill(skill, out var skillNode))
			return;

		_chosenSkill = skillNode;

		if (!_nodeManager.TryGetComponent<TargetingComponent>(skillNode, out var targetingComponent))
			return;

		var firstChildFocused = false;
		switch (targetingComponent.ValidTargets)
		{
			case TargetingComponent.Targets.All:
				break;
			case TargetingComponent.Targets.Allies:
				foreach (var child in _playersVBoxContainer.GetChildren())
				{
					if (child is not CombatMobRepresentationSystem combatMobRepresentationSystem)
						continue;
					
					combatMobRepresentationSystem.Target.SetVisible(true);
					
					if (firstChildFocused)
						continue;
					
					combatMobRepresentationSystem.Target.GrabFocus();
					firstChildFocused = true;
				}
				break;
			case TargetingComponent.Targets.Enemies:
				foreach (var child in _enemiesVBoxContainer.GetChildren())
				{
					if (child is not CombatMobRepresentationSystem combatMobRepresentationSystem)
						continue;
					
					combatMobRepresentationSystem.Target.SetVisible(true);
					
					if (firstChildFocused)
						continue;
					
					combatMobRepresentationSystem.Target.GrabFocus();
					firstChildFocused = true;
				}
				break;
		}
	}

	/// <summary>
	/// When a target is chosen,
	/// apply the chosen skill's combat effects on the target
	/// and then proceed the turn order
	/// </summary>
	/// <param name="target"></param>
	private void OnTargetChosen(Node target)
	{
		if (_chosenSkill == null)
			return;

		// Emit a signal indicating which skill was used and who is being targeted by the skill
		if (!_nodeManager.TryGetComponent<SkillComponent>(_chosenSkill, out var skillComponent))
			return;

		if (_turnManagerSystem.CurrentMobsTurn == null)
			return;
		
		var useSkillSignal = new UseSkillSignal(_turnManagerSystem.CurrentMobsTurn, target);
		_nodeManager.SignalBus.EmitUseSkillSignal((_chosenSkill, skillComponent), ref useSkillSignal);
		
		var endOfTurnSignal = new EndOfTurnSignal();
		_nodeManager.SignalBus.EmitEndOfTurnSignal(_turnManagerSystem.CurrentMobsTurn, ref endOfTurnSignal);
	}
	
	/// <summary>
	/// Shows the names and health bars for each given group of mobs
	/// TODO: Rename _nameAndHealthBar and the name of this function to reflect that they manage each mob representation and their ability to act as a target
	/// </summary>
	/// <param name="container">Which side of combat the mob belongs to</param>
	/// <param name="mobs"></param>
	private void LoadHealthBars(VBoxContainer container, Array<Node> mobs)
	{
		// Clear out any existing children from template
		foreach (var child in container.GetChildren())
			child.QueueFree();
		
		foreach (var mob in mobs)
		{
			var node = _combatMobRepresentation.Instantiate<CombatMobRepresentationSystem>();
			node.SetMob(mob);
			container.AddChild(node);
			node.TargetPressed += OnTargetChosen;
		}
	}
}