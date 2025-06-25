using CS.Components;
using CS.Components.Damageable;
using CS.Components.Description;
using CS.Nodes.Skills.Manager;
using Godot;
using Godot.Collections;

namespace CS.Nodes.Combat.Skills;

/// <summary>
/// TODO: EXTREMELY LATER ON REFACTOR, USE BUTTONS INSTEAD OF ITEMLIST, EACH BUTTON WILL HOLD THE SKILL
/// </summary>
public partial class CombatSkillsItemListSystem : ItemList
{
	private SkillManagerSceneSystem? _skillManagerSceneSystem;
	[Export] private CombatSkillsSceneSystem? _combatSkillsSceneSystem;
	[Export] private Label? _skillName;
	[Export] private Label? _skillDescription;
	[Export] private VBoxContainer? _effectContainer;
	[Export] private VBoxContainer? _costContainer;

	[Signal]
	public delegate void UseSkillEventHandler(Node skill, TargetingComponent.Targets targets);
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// TODO: un-hardcode path
		_skillManagerSceneSystem = GetNode<SkillManagerSceneSystem>("/root/MainScene/SkillManagerScene");
		
		ItemSelected += OnItemSelected;
		ItemActivated += OnItemActivated;
		
		if (_combatSkillsSceneSystem != null)
			_combatSkillsSceneSystem.DisplayPlayerSkills += OnDisplayPlayerSkills;
		else
			GD.PrintErr("Combat skills scene not found\n" + System.Environment.StackTrace);

	}

	private void ClearSkillInfo()
	{
		if (_skillName != null)
			_skillName.Text = "";
		
		if (_skillDescription != null)
			_skillDescription.Text = "";

		if (_effectContainer != null)
		{
			foreach (var node in _effectContainer.GetChildren())
			{
				if (node.Name != "Spacer")
					node.QueueFree();
			}
		}
		
		if (_costContainer != null)
		{
			foreach (var node in _costContainer.GetChildren())
			{
				if (node.Name != "Spacer")
					node.QueueFree();
			}
		}
	}

	private void ShowSkillInfo(string name, string description, Array<string> combatEffects, Array<string> combatCosts)
	{
		ClearSkillInfo();
		
		_skillName?.SetText(name);
		_skillDescription?.SetText(description);

		if (_effectContainer != null)
		{
			foreach (var effect in combatEffects)	
			{
				var label = new Label();
				label.SetText(effect);
				_effectContainer.AddChild(label);
			}	
		}
		
		if (_costContainer != null)
		{
			foreach (var cost in combatCosts)
			{
				var label = new Label();
				label.SetText(cost);
				_costContainer.AddChild(label);
			}	
		}
	}

	private void OnItemActivated(long index)
	{
		var name = GetItemText((int)index);
		
		if (_skillManagerSceneSystem == null || !_skillManagerSceneSystem.TryGetSkill(name, out var skill))
			return;
		
		if (!ComponentSystem.TryGetComponent<TargetingComponent>(skill, out var targetingComponent))
			return;

		EmitSignalUseSkill(skill, targetingComponent.ValidTargets);
	}
	
	private void OnDisplayPlayerSkills(Array<string> skills)
	{
		Clear();
		ClearSkillInfo();

		if (_skillManagerSceneSystem == null)
			return;
		
		// Prevents adding invalid skills to battle options
		foreach (var skill in skills)
		{
			GD.Print(skill);
			if (_skillManagerSceneSystem.SkillExists(skill))
				AddItem(skill);
		}
		
		GrabFocus();
		Select(0);
		OnItemSelected(0);
	}

	private void OnItemSelected(long index)
	{
		var name = GetItemText((int)index);
		
		if (_skillManagerSceneSystem == null || !_skillManagerSceneSystem.TryGetSkill(name, out var skill))
			return;
		
		if (!ComponentSystem.TryGetComponent<DescriptionComponent>(skill, out var descriptionComponent))
			return;
		
		ShowSkillInfo(descriptionComponent.DisplayName, descriptionComponent.Description, descriptionComponent.CombatEffects, descriptionComponent.CombatCosts);
	}
}