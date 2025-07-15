using CS.Components.CombatManager;
using CS.Components.Damageable;
using CS.Components.Magic;
using CS.Components.Mob;
using CS.Components.Skills;
using CS.Nodes.Scenes.Combat.SkillSelection;
using CS.Nodes.Scenes.Combat.SpellSelection;
using CS.Nodes.UI.Audio;
using CS.Resources.CombatScenarios;
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
	private MagicSystem _magicSystem = default!;
	private SkillSystem _skillSystem = default!;
	private TurnManagerSystem _turnManagerSystem = default!;
	private CombatScenarioSystem _combatScenarioSystem = default!;
	
	private Node? _chosenSkill;
	
	[ExportCategory("Instantiated")]
	[Export] private AudioStream? _bgm;
	[Export] public Array<Node> Enemies = [];
	[Export] public Array<Node> Players = [];

	[ExportCategory("Owned")]
	[Export] private ActionsItemListSystem _actionsItemList = default!;
	[Export] private PackedScene _combatMobRepresentation = default!;
	[Export] private PackedScene _combatSkillSelection = default!;
	[Export] private PackedScene _combatSpellSelection = default!;
	[Export] private VBoxContainer _enemiesVBoxContainer = default!;
	[Export] private VBoxContainer _playersVBoxContainer = default!;
	
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
		var bgmPlayer = GetNode<LoopingAudioStreamPlayer2DSystem>("/root/MainScene/BGMAudioStreamPlayer2D");
		if (bgmPlayer != null)
		{
			bgmPlayer.SetStream(_bgm);
			bgmPlayer.Play();
		}

		if (_nodeSystemManager.TryGetNodeSystem<CombatScenarioSystem>(out var combatScenarioSystem))
			_combatScenarioSystem = combatScenarioSystem;
		
		var mobs = _combatScenarioSystem.GetNextEnemies();
		foreach (var mob in mobs)
		{
			AddChild(mob);
			mob.SetOwner(this);
			Enemies.Add(mob);
		}
		_nodeManager.SignalBus.MobDiedSignal += OnMobDied;
		_nodeManager.SignalBus.StartOfTurnSignal += OnStartOfTurn;
		_actionsItemList.SkillsPressed += OnSkillsPressed;
		_actionsItemList.SpellsPressed += OnSpellsPressed;
		_actionsItemList.FleePressed += OnFleePressed;
		_actionsItemList.SetVisible(false);

		if (_nodeSystemManager.TryGetNodeSystem<MagicSystem>(out var magicSystem))
			_magicSystem = magicSystem;
			
		if (_nodeSystemManager.TryGetNodeSystem<SkillSystem>(out var skillManagerSystem))
			_skillSystem = skillManagerSystem;

		if (_nodeSystemManager.TryGetNodeSystem<TurnManagerSystem>(out var turnManagerSystem))
			_turnManagerSystem = turnManagerSystem;
		
		var signal = new StartOfCombatSignal(Players, Enemies);
		_nodeManager.SignalBus.EmitStartOfCombatSignal(this, ref signal);
		
		LoadHealthBars(_playersVBoxContainer, Players);
		LoadHealthBars(_enemiesVBoxContainer, Enemies);
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		_nodeManager.SignalBus.MobDiedSignal -= OnMobDied;
		_nodeManager.SignalBus.StartOfTurnSignal -= OnStartOfTurn;
	}

	private void OnFleePressed()
	{
		var signal = new EndOfCombatSignal();
		_nodeManager.SignalBus.EmitEndOfCombatSignal(this, ref signal);
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

	/// <summary>
	/// On death remove mob from players or enemies
	/// as well as turn order
	/// </summary>
	private void OnMobDied(Node node, ref MobDiedSignal args)
	{
        Players.Remove(node);
        Enemies.Remove(node);

        var signal = new EndOfCombatSignal();
        if (Enemies.Count == 0)
	        signal.Won = true;
        
        if (Players.Count == 0 || Enemies.Count == 0)
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
		if (Players.Count == 0 || Enemies.Count == 0)
			return;
		
		if (Players.Contains(node))
		{
			_actionsItemList.SetVisible(true);
			return;
		}

		if (!_nodeManager.TryGetComponent<MobComponent>(node, out var mobComponent))
			return;
		
		var enemyTurnSignal = new EnemyTurnSignal(Players, Enemies);
		_nodeManager.SignalBus.EmitEnemyTurnSignal((node, mobComponent), ref enemyTurnSignal);
		
		var endOfTurnSignal = new EndOfTurnSignal();
		_nodeManager.SignalBus.EmitEndOfTurnSignal(node, ref endOfTurnSignal);
	}

	/// <summary>
	/// Show off the skill selection scene when skill option is selected
	/// </summary>
	private void OnSkillsPressed()
	{
		if (_turnManagerSystem.CurrentMobsTurn == null)
			return;
		
		_actionsItemList.SetVisible(false);
		_skillSystem.GetKnownSkills(_turnManagerSystem.CurrentMobsTurn, out var skills);
		
		var node = _combatSkillSelection.Instantiate<SkillSelectionSceneSystem>();
		AddChild(node);
		node.SetSkills(skills);
		node.EscapePressed += OnEscapePressed;
		node.SkillChosen += OnSkillChosen;
	}

	/// <summary>
	/// When a skill is chosen from the list,
	/// show off targets based on the skill's <see cref="TargetingComponent"/> setting
	/// </summary>
	/// <param name="skill"></param>
	private void OnSkillChosen(string skill)
	{
		if (!_skillSystem.TryGetSkill(skill, out var skillNode))
			return;

		_chosenSkill = skillNode;

		if (!_nodeManager.TryGetComponent<TargetingComponent>(skillNode, out var targetingComponent))
			return;

		SetTargets(targetingComponent.ValidTargets, false, true);
	}

	private void OnSpellsPressed()
	{
		if (_turnManagerSystem.CurrentMobsTurn == null)
			return;
		
		_actionsItemList.SetVisible(false);
		
		_magicSystem.GetKnownSpells(_turnManagerSystem.CurrentMobsTurn, out var spells);

		var node = _combatSpellSelection.Instantiate<SpellSelectionSceneSystem>();
		AddChild(node);
		node.SetSpells(spells);
		node.EscapePressed += OnEscapePressed;
		node.SpellChosen += OnSpellChosen;
	}

	private void OnSpellChosen(string spell)
	{
		
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
		
		if (!_nodeManager.TryGetComponent<TargetingComponent>(_chosenSkill, out var targetingComponent))
			return;

		SetTargets(targetingComponent.ValidTargets, true);

		if (_turnManagerSystem.CurrentMobsTurn == null)
			return;
		
		var useSkillSignal = new UseSkillSignal(_turnManagerSystem.CurrentMobsTurn, target);
		_nodeManager.SignalBus.EmitUseSkillSignal((_chosenSkill, skillComponent), ref useSkillSignal);
		
		var endOfTurnSignal = new EndOfTurnSignal();
		_nodeManager.SignalBus.EmitEndOfTurnSignal(_turnManagerSystem.CurrentMobsTurn, ref endOfTurnSignal);
	}

	private void SetTargets(TargetingComponent.Targets targets, bool disable, bool focus = false)
	{
		var firstChildFocused = false;
		switch (targets)
		{
			case TargetingComponent.Targets.All:
				break;
			case TargetingComponent.Targets.Allies:
				foreach (var child in _playersVBoxContainer.GetChildren())
				{
					if (child is not CombatMobRepresentationSystem combatMobRepresentationSystem)
						continue;
					
					combatMobRepresentationSystem.MobNameLinkButton.SetDisabled(disable);
					combatMobRepresentationSystem.MobNameLinkButton.FocusMode = disable ? FocusModeEnum.None : FocusModeEnum.All;

					
					if (!focus || firstChildFocused)
						continue;
					
					combatMobRepresentationSystem.MobNameLinkButton.GrabFocus();
					firstChildFocused = true;
				}
				break;
			case TargetingComponent.Targets.Enemies:
				foreach (var child in _enemiesVBoxContainer.GetChildren())
				{
					if (child is not CombatMobRepresentationSystem combatMobRepresentationSystem)
						continue;
					
					combatMobRepresentationSystem.MobNameLinkButton.SetDisabled(disable);
					combatMobRepresentationSystem.MobNameLinkButton.FocusMode = disable ? FocusModeEnum.None : FocusModeEnum.All;
					
					if (!focus || firstChildFocused)
						continue;
					
					combatMobRepresentationSystem.MobNameLinkButton.GrabFocus();
					firstChildFocused = true;
				}
				break;
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