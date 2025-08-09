using CS.Components.Description;
using CS.Components.Magic;
using CS.Components.Mob;
using CS.Nodes.UI.Tooltip;
using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Nodes.Scenes.Combat.SpellSelection;

public partial class SpellSelectionSceneSystem : Control
{
	private readonly NodeManager _nodeManager = NodeManager.Instance;
	private DescriptionSystem _descriptionSystem = null!;
	private MagicSystem _magicSystem = null!;
	
	/// <summary>
	/// Key is the display name, value is the actual skill node
	/// </summary>
	private Dictionary<string, Node> _spells = [];
	
	[ExportCategory("Owned")]
	[Export] private PackedScene _customTooltip = null!;
	[Export] private SpellSelectionItemListSystem _spellSelectionItemListSystem = null!;
	[Export] private Label _spellName = null!;
	[Export] private Label _spellDescription = null!;
	[Export] private VBoxContainer _effectContainer = null!;
	[Export] private VBoxContainer _costContainer = null!;
	
	[Signal]
	public delegate void SpellChosenEventHandler(Node node);
	
	[Signal]
	public delegate void EscapePressedEventHandler(Node node);
	
	public override void _Ready()
	{
		if (NodeSystemManager.Instance.TryGetNodeSystem<DescriptionSystem>(out var descriptionSystem))
			_descriptionSystem = descriptionSystem;
		
		if (NodeSystemManager.Instance.TryGetNodeSystem<MagicSystem>(out var nodeSystem))
			_magicSystem = nodeSystem;

		_spellSelectionItemListSystem.PreviewSpell += OnPreviewSpell;
		_spellSelectionItemListSystem.ItemActivated += OnItemActivated;
		_spellSelectionItemListSystem.EscapePressed += OnEscapePressed;
	}

	private void OnEscapePressed()
	{
		EmitSignalEscapePressed(this);
	}
	
	/// <summary>
	/// When an item is chosen, we communicate that the user wants to use this skill
	/// </summary>
	/// <param name="index"></param>
	private void OnItemActivated(long index)
	{
		if (!_spells.TryGetValue(_spellSelectionItemListSystem.GetItemText((int)index), out var spellNode))
			return;
		
		EmitSignalSpellChosen(spellNode);
		SetVisible(false);
	}
	
	private void OnMetaHovered(Variant name)
	{
		if (!_nodeManager.NodeDictionary.TryGetValue(name.AsString(), out var node))
			return;

		var tooltip = _customTooltip.Instantiate<CustomTooltip>();
		tooltip.SetTooltipTitle(_descriptionSystem.GetDisplayName(node));
		tooltip.SetTooltipDescription(_descriptionSystem.GetDescription(node));
		// tooltip.SetTooltipBulletpoints(_descriptionSystem.GetEffects(node));
		AddChild(tooltip);
		var mousePosition = GetViewport().GetMousePosition();
		tooltip.Popup(new Rect2I((int)mousePosition.X - 16, (int)mousePosition.Y - 16, 0, 0));
	}

	/// <summary>
	/// Displays the name, description, and other information related to the skill in further detail
	/// </summary>
	/// <param name="spellName">The skill to be further inspected</param>
	private void OnPreviewSpell(string spellName)
	{
		if (!_spells.TryGetValue(spellName, out var spellNode))
			return;
		
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

		_spellName.SetText(_descriptionSystem.GetDisplayName(spellNode));
		_spellDescription.SetText(_descriptionSystem.GetDescription(spellNode));
		
		foreach (var effect in _descriptionSystem.GetEffects(spellNode))	
		{
			var label = new RichTextLabel();
			label.BbcodeEnabled = true;
			label.SetFitContent(true);
			label.SetText(effect);
			label.MetaHoverStarted += OnMetaHovered;
			_effectContainer.AddChild(label);
		}
		
		foreach (var cost in _descriptionSystem.GetCosts(spellNode))
		{
			var label = new RichTextLabel();
			label.BbcodeEnabled = true;
			label.SetFitContent(true);
			label.SetText(cost);
			label.SetHorizontalAlignment(HorizontalAlignment.Right);
			_costContainer.AddChild(label);
		}
	}

	/// <summary>
	/// Fills up the available skill list which can be browsed through to display a detailed preview of what the skill does
	/// </summary>
	/// <param name="spells">A list of skills that the mob can use</param>
	public void SetSpells(Array<string> spells, Node mob)
	{
		_spellSelectionItemListSystem.Clear();
		foreach (var spellName in spells)
		{
			if (!_magicSystem.TryGetSpell(spellName, out var spellNode))
				return;

			if (!_nodeManager.TryGetComponent<SpellComponent>(spellNode, out var spellComponent))
				return;

			if (!_nodeManager.TryGetComponent<DescriptionComponent>(spellNode, out var descriptionComponent))
				return;

			if (!_nodeManager.TryGetComponent<MobComponent>(mob, out var mobComponent))
				return;
			
			_spells.Add(descriptionComponent.DisplayName, spellNode);
			var isCastable = _magicSystem.IsSpellCastable((mob, mobComponent), (spellNode, spellComponent));
			_spellSelectionItemListSystem?.AddItem(descriptionComponent.DisplayName, selectable: isCastable);
		}
		
		_spellSelectionItemListSystem?.GrabFocus();
		_spellSelectionItemListSystem?.Select(0);
		_spellSelectionItemListSystem?.OnItemSelected(0);
	}
}