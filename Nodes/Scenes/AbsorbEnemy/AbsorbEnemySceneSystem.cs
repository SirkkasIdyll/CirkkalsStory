using CS.Components.CombatManager;
using CS.Components.Description;
using CS.Components.Mob;
using CS.Components.Player;
using CS.Nodes.UI.ButtonTypes;
using CS.Nodes.UI.DialogBox;
using CS.SlimeFactory;
using Godot;

namespace CS.Nodes.Scenes.AbsorbEnemy;

public partial class AbsorbEnemySceneSystem : Control
{
	private readonly NodeManager _nodeManager = NodeManager.Instance;
	private readonly NodeSystemManager _nodeSystemManager = NodeSystemManager.Instance;
	private DescriptionSystem _descriptionSystem = null!;
	private PlayerManagerSystem _playerManagerSystem = null!;
	private ScenarioSystem _scenarioSystem = null!;

	private Node _enemyToAbsorb = null!;
	private string _enemyAbility = "";
	private string _enemySkill = "";
	private string _enemySpell = "";

	[ExportCategory("Instantiated")]
	[Export] private PackedScene? _nextScene;

	[ExportCategory("Owned")]
	[Export] private MarginContainer _contentContainer = null!;
	[Export] private PackedScene _dialogBox = null!;
	[Export] private StandardButton _abilityButton = null!;
	[Export] private StandardButton _skillButton = null!;
	[Export] private StandardButton _spellButton = null!;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (_nodeSystemManager.TryGetNodeSystem<DescriptionSystem>(out var descriptionSystem))
			_descriptionSystem = descriptionSystem;
		
		if (_nodeSystemManager.TryGetNodeSystem<PlayerManagerSystem>(out var playerManagerSystem))
			_playerManagerSystem = playerManagerSystem;
		
		if (_nodeSystemManager.TryGetNodeSystem<ScenarioSystem>(out var scenarioSystem))
			_scenarioSystem = scenarioSystem;

		CompareUserToEnemy();
		_abilityButton.Pressed += OnAbilityButtonPressed;
		_skillButton.Pressed += OnSkillButtonPressed;
		_spellButton.Pressed += OnSpellButtonPressed;
	}

	private void OnAbilityButtonPressed()
	{
		_contentContainer.SetVisible(false);
		
		if (!_nodeManager.TryGetComponent<MobComponent>(_playerManagerSystem.GetPlayer(), out var playerMobComponent))
			return;
		
		playerMobComponent.Abilities.Add(_enemyAbility);
		
		if (!_nodeManager.NodeDictionary.TryGetValue(_enemyAbility, out var ability))
			return;
		
		/*var summary = "You gained the ability [b]" + _descriptionSystem.GetDisplayName(ability) + "[/b] from the [b]" +
		              _descriptionSystem.GetDisplayName(_enemyToAbsorb) + ".";*/
		
		var dialogBox = _dialogBox.Instantiate<DialogBox>();
		// dialogBox.SetDetails("Ability Absorption", summary);
		dialogBox.DialogFinished += () =>
		{
			if (_nextScene != null)
			{
				var signal = new ChangeActiveSceneSignal(_nextScene);
				_nodeManager.SignalBus.EmitChangeActiveSceneSignal(this, ref signal);
			}
		};
		AddChild(dialogBox);
	}

	private void OnSkillButtonPressed()
	{
		_contentContainer.SetVisible(false);
		
		if (!_nodeManager.TryGetComponent<MobComponent>(_playerManagerSystem.GetPlayer(), out var playerMobComponent))
			return;
		
		playerMobComponent.Skills.Add(_enemySkill);
		
		if (!_nodeManager.NodeDictionary.TryGetValue(_enemySkill, out var skill))
			return;
		
		/*var summary = "You gained the skill [b]" + _descriptionSystem.GetDisplayName(skill) + "[/b] from the [b]" +
		              _descriptionSystem.GetDisplayName(_enemyToAbsorb) + ".";*/
		
		var dialogBox = _dialogBox.Instantiate<DialogBox>();
		// dialogBox.SetDetails("Skill Absorption", summary);
		dialogBox.DialogFinished += () =>
		{
			if (_nextScene != null)
			{
				var signal = new ChangeActiveSceneSignal(_nextScene);
				_nodeManager.SignalBus.EmitChangeActiveSceneSignal(this, ref signal);
			}
		};
		AddChild(dialogBox);
	}

	private void OnSpellButtonPressed()
	{
		_contentContainer.SetVisible(false);
		
		if (!_nodeManager.TryGetComponent<MobComponent>(_playerManagerSystem.GetPlayer(), out var playerMobComponent))
			return;
		
		playerMobComponent.Spells.Add(_enemySpell);
		
		if (!_nodeManager.NodeDictionary.TryGetValue(_enemySpell, out var spell))
			return;
		
		/*var summary = "You gained the spell [b]" + _descriptionSystem.GetDisplayName(spell) + "[/b] from the [b]" +
		              _descriptionSystem.GetDisplayName(_enemyToAbsorb) + ".";*/
		
		var dialogBox = _dialogBox.Instantiate<DialogBox>();
		// dialogBox.SetDetails("Spell Absorption", summary);
		dialogBox.DialogFinished += () =>
		{
			if (_nextScene != null)
			{
				var signal = new ChangeActiveSceneSignal(_nextScene);
				_nodeManager.SignalBus.EmitChangeActiveSceneSignal(this, ref signal);
			}
		};
		AddChild(dialogBox);
	}

	private void CompareUserToEnemy()
	{
		_nodeManager.NodeDictionary.TryGetValue(_scenarioSystem.PreviousEnemies.PickRandom(), out _enemyToAbsorb!);

		if (!_nodeManager.TryGetComponent<MobComponent>(_enemyToAbsorb, out var enemyMobComponent))
			return;
		
		if (!_nodeManager.TryGetComponent<MobComponent>(_playerManagerSystem.GetPlayer(), out var playerMobComponent))
			return;

		var possibleAbilities = enemyMobComponent.Abilities.Duplicate();
		foreach (var ability in playerMobComponent.Abilities)
			possibleAbilities.Remove(ability);

		if (possibleAbilities.Count == 0)
		{
			_abilityButton.Disabled = true;
			_abilityButton.FocusMode = FocusModeEnum.None;
		}
		else
		{
			_enemyAbility = possibleAbilities.PickRandom();
		}

		var possibleSkills = enemyMobComponent.Skills.Duplicate();
		foreach (var skill in playerMobComponent.Skills)
			possibleSkills.Remove(skill);

		if (possibleSkills.Count == 0)
		{
			_skillButton.Disabled = true;
			_skillButton.FocusMode = FocusModeEnum.None;
		}
		else
		{
			_enemySkill =  possibleSkills.PickRandom();
		}
		
		var possibleSpells = enemyMobComponent.Spells.Duplicate();
		foreach (var spell in playerMobComponent.Spells)
			possibleSpells.Remove(spell);

		if (possibleSpells.Count == 0)
		{
			_spellButton.Disabled = true;
			_spellButton.FocusMode = FocusModeEnum.None;
		}
		else
		{
			_enemySpell  = possibleSpells.PickRandom();
		}
	}
}