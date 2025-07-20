using CS.Components.CombatManager;
using CS.Components.Damage;
using CS.Components.Damageable;
using CS.Components.Description;
using CS.Components.Magic;
using CS.Components.Mob;
using CS.Components.Player;
using CS.Components.Skills;
using CS.Nodes.Scenes.Combat.SkillSelection;
using CS.Nodes.Scenes.Combat.SpellSelection;
using CS.Nodes.UI.Audio;
using CS.Nodes.UI.DialogBox;
using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Nodes.Scenes.Combat;

public partial class CombatSceneSystem : Control
{
	private readonly NodeManager _nodeManager = NodeManager.Instance;
	private readonly NodeSystemManager _nodeSystemManager = NodeSystemManager.Instance;
	private ScenarioSystem _scenarioSystem = null!;
	private DescriptionSystem _descriptionSystem = null!;
	private MagicSystem _magicSystem = null!;
	private PlayerManagerSystem _playerManagerSystem = null!;
	private SkillSystem _skillSystem = null!;
	private TurnManagerSystem _turnManagerSystem = null!;

	private Node? _chosenAction;
	
	[ExportCategory("Instantiated")]
	[Export] private AudioStream? _bgm;

	[ExportCategory("Owned")]
	[Export] private ActionsItemListSystem _actionsItemList = null!;
	[Export] private PackedScene _combatMobRepresentation = null!;
	[Export] private PackedScene _combatSkillSelection = null!;
	[Export] private PackedScene _combatSpellSelection = null!;
	[Export] private PackedScene _dialogBox = null!;
	[Export] private VBoxContainer _enemiesVBoxContainer = null!;
	[Export] private VBoxContainer _playersVBoxContainer = null!;
	
	private void _InjectDependencies()
	{
		if (_nodeSystemManager.TryGetNodeSystem<ScenarioSystem>(out var combatScenarioSystem))
			_scenarioSystem = combatScenarioSystem;
		
		if (_nodeSystemManager.TryGetNodeSystem<DescriptionSystem>(out var descriptionSystem))
			_descriptionSystem = descriptionSystem;
		
		if (_nodeSystemManager.TryGetNodeSystem<MagicSystem>(out var magicSystem))
			_magicSystem = magicSystem;

		if (_nodeSystemManager.TryGetNodeSystem<PlayerManagerSystem>(out var playerSystem))
			_playerManagerSystem = playerSystem;
			
		if (_nodeSystemManager.TryGetNodeSystem<SkillSystem>(out var skillManagerSystem))
			_skillSystem = skillManagerSystem;

		if (_nodeSystemManager.TryGetNodeSystem<TurnManagerSystem>(out var turnManagerSystem))
			_turnManagerSystem = turnManagerSystem;
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_InjectDependencies();
		
		var bgmPlayer = GetNode<LoopingAudioStreamPlayer2DSystem>("/root/MainScene/BGMAudioStreamPlayer2D");
		if (bgmPlayer != null)
		{
			bgmPlayer.SetStream(_bgm);
			bgmPlayer.Play();
		}

		var player = _playerManagerSystem.GetPlayer();
		_turnManagerSystem.Players.Add(player);
		var enemies = _scenarioSystem.GetNextEnemies();
		foreach (var enemy in enemies)
		{
			AddChild(enemy);
			// enemy.SetOwner(this);
			_turnManagerSystem.Enemies.Add(enemy);
		}
		
		_actionsItemList.FleePressed += OnFleePressed;
		_actionsItemList.SkillsPressed += OnSkillsPressed;
		_actionsItemList.SpellsPressed += OnSpellsPressed;
		_nodeManager.SignalBus.StartOfCombatSignal += OnStartOfCombat;
		_nodeManager.SignalBus.StartOfTurnSignal += OnStartOfTurn;
		_nodeManager.SignalBus.EndOfTurnSignal += OnEndOfTurn;
		_actionsItemList.SetVisible(false);
		LoadHealthBars(_playersVBoxContainer, _turnManagerSystem.Players);
		LoadHealthBars(_enemiesVBoxContainer, _turnManagerSystem.Enemies);
		_turnManagerSystem.StartCombat();
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		_nodeManager.SignalBus.StartOfCombatSignal -= OnStartOfCombat;
		_nodeManager.SignalBus.StartOfTurnSignal -= OnStartOfTurn;
		_nodeManager.SignalBus.EndOfTurnSignal -= OnEndOfTurn;
	}

	private void OnEscapePressedCancelTargeting()
	{
		if (_chosenAction != null)
			HideActionTargets(_chosenAction);
		
		_actionsItemList.SetVisible(true);
	}
	
	/// <summary>
	/// Closes the skill list and opens up the actions list
	/// </summary>
	private void OnEscapePressedClosePopup(Node node)
	{
		node.QueueFree();
		_actionsItemList.SetVisible(true);
	}
	
	private void OnFleePressed()
	{
		_turnManagerSystem.EndCombat(this);
	}
	
	/// <summary>
	/// Show off the skill selection scene when skill option is selected
	/// </summary>
	private void OnSkillsPressed()
	{
		_actionsItemList.SetVisible(false);
		_skillSystem.GetKnownSkills(_turnManagerSystem.GetActiveMob(), out var skills);
		
		var node = _combatSkillSelection.Instantiate<SkillSelectionSceneSystem>();
		AddChild(node);
		node.SetSkills(skills);
		node.EscapePressed += OnEscapePressedClosePopup;
		node.SkillChosen += OnSkillChosen;
	}

	/// <summary>
	/// When a skill is chosen from the list,
	/// show off targets based on the skill's <see cref="TargetingComponent"/> setting
	/// </summary>
	private void OnSkillChosen(Node skill)
	{
		ShowActionTargets(skill);
	}

	private void OnSpellsPressed()
	{
		_actionsItemList.SetVisible(false);
		_magicSystem.GetKnownSpells(_turnManagerSystem.GetActiveMob(), out var spells);

		var node = _combatSpellSelection.Instantiate<SpellSelectionSceneSystem>();
		AddChild(node);
		node.SetSpells(spells);
		node.EscapePressed += OnEscapePressedClosePopup;
		node.SpellChosen += OnSpellChosen;
	}

	private void OnSpellChosen(Node spell)
	{
		ShowActionTargets(spell);
	}

	private void OnStartOfCombat(ref StartOfCombatSignal args)
	{
		_turnManagerSystem.StartTurn();
	}

	private void OnStartOfTurn(Node<MobComponent> node, ref StartOfTurnSignal args)
	{
		var title = _descriptionSystem.GetDisplayName(node);
		var dialogBox = _dialogBox.Instantiate<DialogBox>();
		dialogBox.SetDetails(title, args.Summaries);
		dialogBox.DialogFinished += () =>
		{
			if (_turnManagerSystem.Players.Count == 0 || _turnManagerSystem.Enemies.Count == 0)
			{
				_turnManagerSystem.EndCombat(this);
				return;
			}
			
			if (!_nodeManager.TryGetComponent<HealthComponent>(node, out var healthComponent))
				return;
			
			// Somebody died from a status effect, start the next mob's turn
			if (healthComponent.Health <= 0)
				_turnManagerSystem.StartTurn();
		
			if (_turnManagerSystem.Players.Contains(node))
			{
				_actionsItemList.SetVisible(true);
				return;
			}

			if (_turnManagerSystem.Enemies.Contains(node))
			{
				var enemyTurnSignal = new EnemyTurnSignal(_turnManagerSystem.Players, _turnManagerSystem.Enemies);
				_nodeManager.SignalBus.EmitEnemyTurnSignal(node, ref enemyTurnSignal);
				OnEnemyTurn(node, ref enemyTurnSignal);
				return;
			}
		};
		AddChild(dialogBox);
	}

	private void OnEnemyTurn(Node<MobComponent> node, ref EnemyTurnSignal args)
	{
		if (_turnManagerSystem.Players.Count == 0 || _turnManagerSystem.Enemies.Count == 0)
		{
			_turnManagerSystem.EndCombat(this);
			return;
		}
		
		var title = _descriptionSystem.GetDisplayName(_turnManagerSystem.GetActiveMob());
		var dialogBox = _dialogBox.Instantiate<DialogBox>();
		dialogBox.SetDetails(title, args.Summaries);
		dialogBox.Title.Set("theme_override_colors/font_color", new Color(0.877f, 0.219f, 0.208f));
		dialogBox.DialogFinished += () => _turnManagerSystem.EndTurn();
		AddChild(dialogBox);
	}

	private void OnEndOfTurn(Node<MobComponent> node, ref EndOfTurnSignal args)
	{
		if (_turnManagerSystem.Players.Count == 0 || _turnManagerSystem.Enemies.Count == 0)
		{
			_turnManagerSystem.EndCombat(this);
			return;
		}
		
		var title = _descriptionSystem.GetDisplayName(_turnManagerSystem.GetActiveMob());
		var dialogBox = _dialogBox.Instantiate<DialogBox>();
		dialogBox.SetDetails(title, args.Summaries);
		dialogBox.DialogFinished += () => _turnManagerSystem.StartTurn();
		AddChild(dialogBox);
	}

	/// <summary>
	/// When a target is chosen,
	/// apply the chosen skill's combat effects on the target
	/// and then proceed the turn order
	/// </summary>
	private void OnTargetChosenByPlayer(Node target)
	{
		if (_chosenAction == null)
			return;

		if (!_nodeManager.TryGetComponent<MobComponent>(_turnManagerSystem.GetActiveMob(), out var mobComponent))
			return;
		
		// An action has been chosen, signal its usage
		var useActionSignal = new UseActionSignal(_chosenAction, [target]);
		useActionSignal.Summaries.Add("Used [b]" + _descriptionSystem.GetDisplayName(_chosenAction!) + "[/b] on [b]" +
		                              _descriptionSystem.GetDisplayName(target) + "[/b].");
		_nodeManager.SignalBus.EmitUseActionSignal((_turnManagerSystem.GetActiveMob(), mobComponent), ref useActionSignal);
		
		// Hide the targets, so they're no longer selectable
		HideActionTargets(_chosenAction);
		
		// Show off the result of the action if any was reported
		if (useActionSignal.Summaries.Count <= 0)
			return;
		
		var dialogBox = _dialogBox.Instantiate<DialogBox>();
		dialogBox.SetDetails(_descriptionSystem.GetDisplayName(_turnManagerSystem.GetActiveMob()), useActionSignal.Summaries);
		AddChild(dialogBox);
		dialogBox.DialogFinished += () => _turnManagerSystem.EndTurn();
	}

	private void OnTargetPreviewByPlayer(Node target)
	{
		if (_chosenAction == null)
			return;
		
		if (!_nodeManager.TryGetComponent<MobComponent>(_turnManagerSystem.GetActiveMob(), out var mobComponent))
			return;
		
		var signal = new PreviewActionSignal(_chosenAction, [target]);
		_nodeManager.SignalBus.EmitPreviewActionSignal((_turnManagerSystem.GetActiveMob(), mobComponent), ref signal);
	}

	private void ShowActionTargets(Node action)
	{
		_chosenAction = action;
		
		if (!_nodeManager.TryGetComponent<TargetingComponent>(action, out var targetingComponent))
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

					if (combatMobRepresentationSystem.HpProgressBar.Value == 0)
						continue;
					
					combatMobRepresentationSystem.MobNameLinkButton.SetDisabled(false);
					combatMobRepresentationSystem.MobNameLinkButton.FocusMode = FocusModeEnum.All;
					
					if (firstChildFocused)
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
					
					if (combatMobRepresentationSystem.HpProgressBar.Value == 0)
						continue;
					
					combatMobRepresentationSystem.MobNameLinkButton.SetDisabled(false);
					combatMobRepresentationSystem.MobNameLinkButton.FocusMode = FocusModeEnum.All;
					
					if (firstChildFocused)
						continue;
					
					combatMobRepresentationSystem.MobNameLinkButton.GrabFocus();
					firstChildFocused = true;
				}
				break;
		}
	}

	private void HideActionTargets(Node action)
	{
		_chosenAction = null;
		
		if (!_nodeManager.TryGetComponent<TargetingComponent>(action, out var targetingComponent))
			return;
		
		switch (targetingComponent.ValidTargets)
		{
			case TargetingComponent.Targets.All:
				break;
			case TargetingComponent.Targets.Allies:
				foreach (var child in _playersVBoxContainer.GetChildren())
				{
					if (child is not CombatMobRepresentationSystem combatMobRepresentationSystem)
						continue;
					
					combatMobRepresentationSystem.MobNameLinkButton.SetDisabled(true);
					combatMobRepresentationSystem.MobNameLinkButton.FocusMode = FocusModeEnum.None;
				}
				break;
			case TargetingComponent.Targets.Enemies:
				foreach (var child in _enemiesVBoxContainer.GetChildren())
				{
					if (child is not CombatMobRepresentationSystem combatMobRepresentationSystem)
						continue;
					
					combatMobRepresentationSystem.MobNameLinkButton.SetDisabled(true);
					combatMobRepresentationSystem.MobNameLinkButton.FocusMode = FocusModeEnum.None;
				}
				break;
		}
	}
	
	/// <summary>
	/// Shows the names and health bars for each given group of mobs
	/// </summary>
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
			node.TargetPressed += OnTargetChosenByPlayer;
			node.TargetPreview += OnTargetPreviewByPlayer;
			node.MobNameLinkButton.EscapePressed += OnEscapePressedCancelTargeting;
		}
	}
}