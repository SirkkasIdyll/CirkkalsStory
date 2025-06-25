using Godot;
using Godot.Collections;

namespace CS.Nodes.Combat;

public partial class CombatSceneSystem : Control
{
	[ExportCategory("Instantiated")]
	[Export] private Array<Node> _players = [];
	[Export] private Array<Node> _enemies = [];

	[ExportCategory("Owned")]
	[Export] private PackedScene? _nameAndHealthBarSceneSystem;
	[Export] private VBoxContainer? _playersVBoxContainer;
	[Export] private VBoxContainer? _enemiesVBoxContainer;
	[Export] private ItemList? _actionsItemList;

	/// <summary>
	/// Loads health bars and names for the players
	/// </summary>
	[Signal]
	public delegate void LoadPlayersIntoCombatEventHandler(Array<Node2D> players);

	/// <summary>
	///  Loads health bars and names for the enemies
	/// </summary>
	[Signal]
	public delegate void LoadEnemiesIntoCombatEventHandler(Array<Node2D> enemies);

	/// <summary>
	/// After both sides attack, display the player actions once more
	/// </summary>
	[Signal]
	public delegate void DisplayPlayerActionsEventHandler();

	/// <summary>
	/// If a skill is selected, we want to display the details of it in another UI element
	/// </summary>
	[Signal]
	public delegate void PreviewSkillInfoEventHandler(Node2D skill);

	/// <summary>
	/// If a skill is chosen, we need to begin choosing the targets for the skill
	/// </summary>
	[Signal]
	public delegate void SkillChosenEventHandler(Node2D skill);
	
	/// <summary>
	/// Once the targets are chosen, we enact the skill on those targets
	/// </summary>
	[Signal]
	public delegate void SkillTargetChosenEventHandler(Node2D attacker, Node2D defender);
	
	/// <summary>
	/// We display a textual summary in a UI element of what happened
	/// Example: Slime attacked wolf!
	/// </summary>
	[Signal]
	public delegate void DisplayActionSummaryEventHandler(string summary);
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		LoadHealthBars();
		
		if (_actionsItemList != null)
			_actionsItemList.SetVisible(true);
		else
			GD.PrintErr("Could not load action list\n" + System.Environment.StackTrace);
	}

	private void LoadHealthBars()
	{
		if (_nameAndHealthBarSceneSystem == null)
		{
			GD.PrintErr("Could not load health bars\n" + System.Environment.StackTrace);
			return;
		}

		foreach (var player in _players)
		{
			var node = _nameAndHealthBarSceneSystem.Instantiate<NameAndHealthBarSceneSystem>();
			node.SetMob(player);
			_playersVBoxContainer?.AddChild(node);
		}
		
		foreach (var enemy in _enemies)
		{
			var node = _nameAndHealthBarSceneSystem.Instantiate<NameAndHealthBarSceneSystem>();
			node.SetMob(enemy);
			_enemiesVBoxContainer?.AddChild(node);
		}
	}
}