using CS.Components.Description;
using CS.Components.Magic;
using CS.Components.Skills;
using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Nodes.Scenes.Combat.SpellSelection;

public partial class SpellSelectionSceneSystem : Control
{
	private readonly NodeManager _nodeManager = NodeManager.Instance;
	private MagicSystem _magicSystem = default!;
	private Array<string> _spells = [];
	
	[ExportCategory("Owned")]
	[Export] private SpellSelectionItemListSystem _spellSelectionItemListSystem = default!;
	[Export] private Label _spellName = default!;
	[Export] private Label _spellDescription = default!;
	[Export] private VBoxContainer _effectContainer = default!;
	[Export] private VBoxContainer _costContainer = default!;
	
	[Signal]
	public delegate void SpellChosenEventHandler(string spell);
	
	[Signal]
	public delegate void EscapePressedEventHandler(Node node);
	
	public override void _Ready()
	{
		if (NodeSystemManager.Instance.TryGetNodeSystem<MagicSystem>(out var nodeSystem))
			_magicSystem = nodeSystem;

		_spellSelectionItemListSystem.PreviewSpell += OnPreviewSpell;
		_spellSelectionItemListSystem.ItemActivated += OnItemActivated;
		_spellSelectionItemListSystem.EscapePressed += OnEscapePressed;
		_nodeManager.SignalBus.ReloadCombatDescriptionSignal += OnReloadCombatDescription;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		_nodeManager.SignalBus.ReloadCombatDescriptionSignal -= OnReloadCombatDescription;
	}

	private void OnEscapePressed()
	{
		EmitSignalEscapePressed(this);
	}

	private void OnReloadCombatDescription(Node<DescriptionComponent> node, ref ReloadCombatDescriptionSignal args)
	{
		foreach (var child in _effectContainer.GetChildren())
		{
			if (child.Name != "Spacer")
				child.QueueFree();
		}

		foreach (var child in _costContainer.GetChildren())
		{
			if (child.Name != "Spacer")
				child.QueueFree();
		}

		_spellName.SetText(node.Comp.DisplayName);
		_spellDescription.SetText(node.Comp.Description);
		
		foreach (var effect in node.Comp.CombatEffects)	
		{
			var label = new Label();
			label.SetText(effect);
			_effectContainer.AddChild(label);
		}

		foreach (var cost in node.Comp.CombatCosts)
		{
			var label = new Label();
			label.SetText(cost);
			_costContainer.AddChild(label);
		}
	}
	
	/// <summary>
	/// When an item is chosen, we communicate that the user wants to use this skill
	/// </summary>
	/// <param name="index"></param>
	private void OnItemActivated(long index)
	{
		EmitSignalSpellChosen(_spellSelectionItemListSystem.GetItemText((int) index).Replace(" ", string.Empty));
		SetVisible(false);
	}

	/// <summary>
	/// Displays the name, description, and other information related to the skill in further detail
	/// </summary>
	/// <param name="spellName">The skill to be further inspected</param>
	private void OnPreviewSpell(string spellName)
	{
		if (!_magicSystem.TryGetSpell(spellName, out var spellNode))
			return;

		if (!NodeManager.Instance.TryGetComponent<DescriptionComponent>(spellNode, out var descriptionComponent))
			return;
		
		descriptionComponent.CombatEffects.Clear();
		descriptionComponent.CombatCosts.Clear();
		
		var signal = new ReloadCombatDescriptionSignal();
		_nodeManager.SignalBus.EmitReloadCombatDescriptionSignal((spellNode, descriptionComponent), ref signal);
	}

	/// <summary>
	/// Fills up the available skill list which can be browsed through to display a detailed preview of what the skill does
	/// </summary>
	/// <param name="spells">A list of skills that the mob can use</param>
	public void SetSpells(Array<string> spells)
	{
		_spellSelectionItemListSystem.Clear();
		foreach (var spellName in spells)
		{
			if (!_magicSystem.TryGetSpell(spellName, out var spellNode))
				return;

			if (!NodeManager.Instance.TryGetComponent<DescriptionComponent>(spellNode, out var descriptionComponent))
				return;
			
			_spellSelectionItemListSystem?.AddItem(descriptionComponent.DisplayName);
		}
		
		_spellSelectionItemListSystem?.GrabFocus();
		_spellSelectionItemListSystem?.Select(0);
		_spellSelectionItemListSystem?.OnItemSelected(0);
	}
}