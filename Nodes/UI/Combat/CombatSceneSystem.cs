using CS.Components.CombatManager;
using CS.Components.Damageable;
using CS.Components.Mob;
using CS.Components.Skills;
using CS.Nodes.UI.Combat.SkillSelection;
using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Nodes.UI.Combat;

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
	private SkillManagerSystem? _skillManagerSystem;
	private TurnManagerSystem? _turnManagerSystem;
	private Node? _chosenSkill;
	
	[ExportCategory("Instantiated")]
	[Export] private Array<Node> _players = [];
	[Export] private Array<Node> _enemies = [];

	[ExportCategory("Owned")]
	[Export] private PackedScene? _nameAndHealthBar;
	[Export] private PackedScene? _combatSkillSelection;
	[Export] private VBoxContainer? _playersVBoxContainer;
	[Export] private VBoxContainer? _enemiesVBoxContainer;
	[Export] private ActionsItemListSystem? _actionsItemList;
	
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
		if (_nameAndHealthBar == null ||
		    _combatSkillSelection == null ||
		    _playersVBoxContainer == null ||
		    _enemiesVBoxContainer == null ||
		    _actionsItemList == null)
			GD.PrintErr("Owned property is null\n" + System.Environment.StackTrace);
		
		_nodeManager.SignalBus.MobDiedSignal += OnMobDied;
		_nodeManager.SignalBus.StartOfTurnSignal += OnStartOfTurn;

		if (_playersVBoxContainer != null)
			LoadHealthBars(_playersVBoxContainer, _players);
		
		if (_enemiesVBoxContainer != null)
			LoadHealthBars(_enemiesVBoxContainer, _enemies);
		
		if (_actionsItemList != null)
			_actionsItemList.SkillsPressed += OnSkillsPressed;
		else
			GD.PrintErr("Could not load action list\n" + System.Environment.StackTrace);
		
		if (_nodeSystemManager.TryGetNodeSystem<SkillManagerSystem>(out var skillManagerSystem))
			_skillManagerSystem = skillManagerSystem;

		if (_nodeSystemManager.TryGetNodeSystem<TurnManagerSystem>(out var turnManagerSystem))
		{
			_turnManagerSystem = turnManagerSystem;

			var signal = new StartOfCombatSignal(_players, _enemies);
			_nodeManager.SignalBus.EmitStartOfCombatSignal(this, ref signal);
		}
	}

	/// <summary>
	/// Shows the names and health bars for each given group of mobs
	/// TODO: Rename _nameAndHealthBar and the name of this function to reflect that they manage each mob representation and their ability to act as a target
	/// </summary>
	/// <param name="container">Which side of combat the mob belongs to</param>
	/// <param name="mobs"></param>
	private void LoadHealthBars(VBoxContainer container, Array<Node> mobs)
	{
		if (_nameAndHealthBar == null)
		{
			GD.PrintErr("Could not load health bars\n" + System.Environment.StackTrace);
			return;
		}

		foreach (var mob in mobs)
		{
			var node = _nameAndHealthBar.Instantiate<NameAndHealthBarSceneSystem>();
			container?.AddChild(node);
			node.SetMob(mob);
			node.TargetChosen += OnTargetChosen;
		}
	}

	/// <summary>
	/// On death remove mob from players or enemies
	/// as well as turn order
	/// </summary>
	private void OnMobDied(Node node, ref MobDiedSignal args)
	{
        _players.Remove(node);
        _enemies.Remove(node);
		GD.Print("Mob has died!");
		
		if (_players.Count == 0)
			GD.Print("All players are dead!");
		
		if (_enemies.Count == 0)
			GD.Print("Combat is over!");
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
			_actionsItemList?.SetVisible(true);
			return;
		}
		
		// Start of enemy decision making logic, move this out of combat scene
		// It has nothing to do with the UI

		if (_skillManagerSystem == null)
			return;
		
		if (!_nodeManager.TryGetComponent<MobComponent>(node, out var mobComponent))
			return;

		var skillName = mobComponent.ChooseRandomSkillOrSpell();
		
		// if the skill doesn't exist, just skip the mob's turn to prevent soft-locks
		if (skillName == null)
		{
			var signal = new EndOfTurnSignal();
			_nodeManager.SignalBus.EmitEndOfTurnSignal(node, ref signal);
			return;
		}

		GD.Print(skillName);

		if (!_skillManagerSystem.TryGetSkill(skillName, out var skill))
		{
			return;
		}

		_chosenSkill = skill;
		OnTargetChosen(_players.PickRandom());
	}

	/// <summary>
	/// Show off the skill selection scene when skill option is selected
	/// </summary>
	private void OnSkillsPressed()
	{
		_actionsItemList?.SetVisible(false);
		
		if (_combatSkillSelection == null)
			return;

		if (!_nodeManager.TryGetComponent<MobComponent>(_players[0], out var mobComponent))
			return;
		
		var node = _combatSkillSelection.Instantiate<CombatSkillSelectionSceneSystem>();
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
		if (_skillManagerSystem == null)
			return;

		if (!_skillManagerSystem.TryGetSkill(skill, out var skillNode))
			return;

		_chosenSkill = skillNode;

		if (!_nodeManager.TryGetComponent<TargetingComponent>(skillNode, out var targetingComponent))
			return;

		switch (targetingComponent.ValidTargets)
		{
			case TargetingComponent.Targets.All:
				break;
			case TargetingComponent.Targets.Allies:
				if (_playersVBoxContainer == null)
					return;
				
				foreach (var child in _playersVBoxContainer.GetChildren())
				{
					if (child.HasMethod(NameAndHealthBarSceneSystem.MethodName.ShowTargetButton))
						child.Call(NameAndHealthBarSceneSystem.MethodName.ShowTargetButton);
				}
				break;
			case TargetingComponent.Targets.Enemies:
				if (_enemiesVBoxContainer == null)
					return;
				
				foreach (var child in _enemiesVBoxContainer.GetChildren())
				{
					if (child.HasMethod(NameAndHealthBarSceneSystem.MethodName.ShowTargetButton))
						child.Call(NameAndHealthBarSceneSystem.MethodName.ShowTargetButton);
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
		if (_chosenSkill == null || _playersVBoxContainer == null || _enemiesVBoxContainer == null || _turnManagerSystem == null)
			return;
		
		// Hide targeting buttons for players and enemies
		foreach (var child in _playersVBoxContainer.GetChildren())
		{
			if (child.HasMethod(NameAndHealthBarSceneSystem.MethodName.HideTargetButton))
				child.Call(NameAndHealthBarSceneSystem.MethodName.HideTargetButton);
		}
		
		foreach (var child in _enemiesVBoxContainer.GetChildren())
		{
			if (child.HasMethod(NameAndHealthBarSceneSystem.MethodName.HideTargetButton))
				child.Call(NameAndHealthBarSceneSystem.MethodName.HideTargetButton);
		}

		// Emit a signal indicating which skill was used and who is being targeted by the skill
		if (!_nodeManager.TryGetComponent<SkillComponent>(_chosenSkill, out var skillComponent))
			return;
		
		var useSkillSignal = new UseSkillSignal(_turnManagerSystem.CurrentMobsTurn, target);
		_nodeManager.SignalBus.EmitUseSkillSignal((_chosenSkill, skillComponent), ref useSkillSignal);
		
		var endOfTurnSignal = new EndOfTurnSignal();
		_nodeManager.SignalBus.EmitEndOfTurnSignal(_turnManagerSystem.CurrentMobsTurn, ref endOfTurnSignal);
	}
}