using CS.Components.Description;
using CS.Components.Skills;
using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Nodes.Scenes.Combat.SkillSelection;

public partial class SkillSelectionSceneSystem : Control
{
	private readonly NodeManager _nodeManager = NodeManager.Instance;
	private SkillSystem _skillSystem = null!;
	
	/// <summary>
	/// Key is the display name, value is the actual skill node
	/// </summary>
	private Dictionary<string, Node> _skills = [];
	
	[ExportCategory("Owned")]
	[Export] private SkillSelectionItemListSystem _skillSelectionItemListSystem = null!;
	[Export] private Label _skillName = null!;
	[Export] private Label _skillDescription = null!;
	[Export] private VBoxContainer _effectContainer = null!;
	[Export] private VBoxContainer _costContainer = null!;
	
	[Signal]
	public delegate void SkillChosenEventHandler(Node node);
	
	[Signal]
	public delegate void EscapePressedEventHandler(Node node);
	
	public override void _Ready()
	{
		if (NodeSystemManager.Instance.TryGetNodeSystem<SkillSystem>(out var nodeSystem))
			_skillSystem = nodeSystem;

		_skillSelectionItemListSystem.PreviewSkill += OnPreviewSkill;
		_skillSelectionItemListSystem.ItemActivated += OnItemActivated;
		_skillSelectionItemListSystem.EscapePressed += OnEscapePressed;
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
		if (!_skills.TryGetValue(_skillSelectionItemListSystem.GetItemText((int)index), out var skillNode))
			return;
		
		EmitSignalSkillChosen(skillNode);
		SetVisible(false);
	}

	/// <summary>
	/// Displays the name, description, and other information related to the skill in further detail
	/// </summary>
	/// <param name="skillName">The skill to be further inspected</param>
	private void OnPreviewSkill(string skillName)
	{
		if (!_skills.TryGetValue(skillName, out var skillNode))
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
		_skillSelectionItemListSystem.Clear();
		foreach (var skillName in skills)
		{
			if (!_skillSystem.TryGetSkill(skillName, out var skillNode))
				return;

			if (!NodeManager.Instance.TryGetComponent<DescriptionComponent>(skillNode, out var descriptionComponent))
				return;
			
			_skills.Add(descriptionComponent.DisplayName, skillNode);
			_skillSelectionItemListSystem?.AddItem(descriptionComponent.DisplayName);
		}
		
		_skillSelectionItemListSystem?.GrabFocus();
		_skillSelectionItemListSystem?.Select(0);
		_skillSelectionItemListSystem?.OnItemSelected(0);
	}
}