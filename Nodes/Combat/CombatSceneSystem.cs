using CS.Components.Damageable;
using CS.Components.Mob;
using CS.Nodes.Combat.SkillSelection;
using CS.Nodes.Combat.TurnManager;
using CS.Nodes.Skills.Manager;
using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Nodes.Combat;

/// <summary>
/// 1. Load mobs and actions in
/// 2. On specific actions pressed, listen to options
/// 3. Once ability chosen, choose target
/// 4. Once target chosen, apply combat effects to the target
/// </summary>
public partial class CombatSceneSystem : Control
{
	
	private SkillManagerSceneSystem? _skillManager;
	private Node? _chosenSkill;
	
	[ExportCategory("Instantiated")]
	[Export] private Array<Node> _players = [];
	[Export] private Array<Node> _enemies = [];

	[ExportCategory("Owned")]
	[Export] private TurnManagerSceneSystem? _turnManager;
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
		if (_turnManager == null ||
		    _nameAndHealthBar == null ||
		    _combatSkillSelection == null ||
		    _playersVBoxContainer == null ||
		    _enemiesVBoxContainer == null ||
		    _actionsItemList == null)
			GD.PrintErr("Owned property is null\n" + System.Environment.StackTrace);
		
		_skillManager = GetNode<SkillManagerSceneSystem>("/root/MainScene/SkillManagerScene");
		
		if (_playersVBoxContainer != null)
			LoadHealthBars(_playersVBoxContainer, _players);
		
		if (_enemiesVBoxContainer != null)
			LoadHealthBars(_enemiesVBoxContainer, _enemies);

		if (_turnManager != null)
		{
			_turnManager.MobsTurn += OnMobsTurn;
			_turnManager.SetTurnOrder(_players, _enemies);
		}
		
		if (_actionsItemList != null)
			_actionsItemList.SkillsPressed += OnSkillsPressed;
		else
			GD.PrintErr("Could not load action list\n" + System.Environment.StackTrace);
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
			node.MobDied += OnMobDied;
		}
	}

	/// <summary>
	/// On death remove mob from players or enemies
	/// as well as turn order
	/// </summary>
	/// <param name="mob"></param>
	private void OnMobDied(Node mob)
	{
        _players.Remove(mob);
        _enemies.Remove(mob);
		_turnManager?.RemoveFromTurnOrder(mob);
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
	/// <param name="mob">The mob whose turn it is</param>
	private void OnMobsTurn(Node mob)
	{
		GD.Print(mob.Name);

		// If either side has no one left, don't let anyone take their turn to prevent infinite loops
		if (_players.Count == 0 || _enemies.Count == 0)
			return;
		
		if (_players.Contains(mob))
		{
			_actionsItemList?.SetVisible(true);
			return;
		}

		if (_skillManager == null)
			return;
		
		if (!NodeManager.TryGetComponent<MobComponent>(mob, out var mobComponent))
			return;

		var skillName = mobComponent.ChooseRandomSkillOrSpell();
		
		if (skillName == null)
		{
			_turnManager?.ProceedTurnOrder();
			return;
		}

		GD.Print(skillName);

		if (!_skillManager.TryGetSkill(skillName, out var skill))
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

		if (!NodeManager.TryGetComponent<MobComponent>(_players[0], out var mobComponent))
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
		if (_skillManager == null)
			return;

		if (!_skillManager.TryGetSkill(skill, out var skillNode))
			return;

		_chosenSkill = skillNode;

		if (!NodeManager.TryGetComponent<TargetingComponent>(skillNode, out var targetingComponent))
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
		if (_chosenSkill == null || _playersVBoxContainer == null || _enemiesVBoxContainer == null)
			return;
		
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

		foreach (var child in _chosenSkill.GetChildren())
		{
			if (child.HasMethod("ApplyCombatEffect"))
				child.Call("ApplyCombatEffect", target);
		}
		
		_turnManager?.ProceedTurnOrder();
	}
}