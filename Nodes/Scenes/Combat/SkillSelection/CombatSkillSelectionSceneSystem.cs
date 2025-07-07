using CS.Components.Description;
using CS.Components.Skills;
using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Nodes.Scenes.Combat.SkillSelection;

public partial class CombatSkillSelectionSceneSystem : Control
{
	private readonly NodeManager _nodeManager = NodeManager.Instance;
	private SkillManagerSystem _skillManagerSystem = default!;
	private Array<string> _skills = [];
	
	[ExportCategory("Owned")]
	[Export] private CombatSkillSelectionItemListSystem _combatSkillSelectionItemListSystem = default!;
	[Export] private Label _skillName = default!;
	[Export] private Label _skillDescription = default!;
	[Export] private VBoxContainer _effectContainer = default!;
	[Export] private VBoxContainer _costContainer = default!;
	
	[Signal]
	public delegate void SkillChosenEventHandler(string skill);
	
	[Signal]
	public delegate void EscapePressedEventHandler(Node node);
	
	public override void _Ready()
	{
		if (NodeSystemManager.Instance.TryGetNodeSystem<SkillManagerSystem>(out var nodeSystem))
			_skillManagerSystem = nodeSystem;

		_combatSkillSelectionItemListSystem.PreviewSkill += OnPreviewSkill;
		_combatSkillSelectionItemListSystem.ItemActivated += OnItemActivated;
		_combatSkillSelectionItemListSystem.EscapePressed += OnEscapePressed;
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

		_skillName.SetText(node.Comp.DisplayName);
		_skillDescription.SetText(node.Comp.Description);

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
		EmitSignalSkillChosen(_combatSkillSelectionItemListSystem.GetItemText((int) index).Replace(" ", string.Empty));
		SetVisible(false);
	}

	/// <summary>
	/// Displays the name, description, and other information related to the skill in further detail
	/// </summary>
	/// <param name="skillName">The skill to be further inspected</param>
	private void OnPreviewSkill(string skillName)
	{
		if (!_skillManagerSystem.TryGetSkill(skillName, out var skillNode))
			return;

		if (!NodeManager.Instance.TryGetComponent<DescriptionComponent>(skillNode, out var descriptionComponent))
			return;

		descriptionComponent.CombatEffects.Clear();
		descriptionComponent.CombatCosts.Clear();
		var signal = new ReloadCombatDescriptionSignal();
		_nodeManager.SignalBus.EmitReloadCombatDescriptionSignal((skillNode, descriptionComponent), ref signal);
	}

	/// <summary>
	/// Fills up the available skill list which can be browsed through to display a detailed preview of what the skill does
	/// </summary>
	/// <param name="skills">A list of skills that the mob can use</param>
	public void SetSkills(Array<string> skills)
	{
		_combatSkillSelectionItemListSystem.Clear();
		foreach (var skillName in skills)
		{
			if (!_skillManagerSystem.TryGetSkill(skillName, out var skillNode))
				return;

			if (!NodeManager.Instance.TryGetComponent<DescriptionComponent>(skillNode, out var descriptionComponent))
				return;
			
			_combatSkillSelectionItemListSystem?.AddItem(descriptionComponent.DisplayName);
		}
		
		_combatSkillSelectionItemListSystem?.GrabFocus();
		_combatSkillSelectionItemListSystem?.Select(0);
		_combatSkillSelectionItemListSystem?.OnItemSelected(0);
	}
}