using CS.Components.Description;
using CS.Components.Skills;
using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Nodes.UI.Combat.SkillSelection;

public partial class CombatSkillSelectionSceneSystem : Control
{
	private SkillManagerSystem? _skillManagerSystem;
	
	[ExportCategory("Instantiated")]
	[Export] public Array<string> Skills = [];
	
	[ExportCategory("Owned")]
	[Export] private CombatSkillSelectionItemListSystem? _combatSkillSelectionItemListSystem;
	[Export] private Label? _skillName;
	[Export] private Label? _skillDescription;
	[Export] private VBoxContainer? _effectContainer;
	[Export] private VBoxContainer? _costContainer;
	
	[Signal]
	public delegate void SkillChosenEventHandler(string skill);
	
	public override void _Ready()
	{
		if (NodeSystemManager.Instance.TryGetNodeSystem<SkillManagerSystem>(out var nodeSystem))
			_skillManagerSystem = nodeSystem;
		
		if (_combatSkillSelectionItemListSystem != null)
		{
			_combatSkillSelectionItemListSystem.PreviewSkill += OnPreviewSkill;
			_combatSkillSelectionItemListSystem.ItemActivated += OnItemActivated;
		}
		else
		{
			GD.PrintErr("Could not find combat skills item list\n" + System.Environment.StackTrace);
		}
	}
	
	/// <summary>
	/// Clears out the previously used skill preview information
	/// </summary>
	private void ClearSkillPreview()
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
	
	/// <summary>
	/// When an item is chosen, we communicate that the user wants to use this skill
	/// </summary>
	/// <param name="index"></param>
	private void OnItemActivated(long index)
	{
		if (_combatSkillSelectionItemListSystem == null)
			return;
		
		EmitSignalSkillChosen(_combatSkillSelectionItemListSystem.GetItemText((int) index));
		SetVisible(false);
	}

	/// <summary>
	/// Displays the name, description, and other information related to the skill in further detail
	/// </summary>
	/// <param name="skill">The skill to be further inspected</param>
	private void OnPreviewSkill(string skill)
	{
		ClearSkillPreview();
		
		if (_skillManagerSystem == null)
			return;
		
		if (!_skillManagerSystem.TryGetSkill(skill, out var skillNode))
			return;

		if (!NodeManager.Instance.TryGetComponent<DescriptionComponent>(skillNode, out var descriptionComponent))
			return;
		
		_skillName?.SetText(descriptionComponent.DisplayName);
		_skillDescription?.SetText(descriptionComponent.Description);
		
		if (_effectContainer != null)
		{
			foreach (var effect in descriptionComponent.CombatEffects)	
			{
				var label = new Label();
				label.SetText(effect);
				_effectContainer.AddChild(label);
			}	
		}
		
		if (_costContainer != null)
		{
			foreach (var cost in descriptionComponent.CombatCosts)
			{
				var label = new Label();
				label.SetText(cost);
				_costContainer.AddChild(label);
			}	
		}
	}

	/// <summary>
	/// Fills up the available skill list which can be browsed through to display a detailed preview of what the skill does
	/// </summary>
	/// <param name="skills">A list of skills that the mob can use</param>
	public void SetSkills(Array<string> skills)
	{
		if (_skillManagerSystem == null)
			return;
		
		Skills = skills;
		_combatSkillSelectionItemListSystem?.Clear();
		foreach (var skill in skills)
		{
			if (!_skillManagerSystem.SkillExists(skill))
				continue;
			
			_combatSkillSelectionItemListSystem?.AddItem(skill);
		}
		
		_combatSkillSelectionItemListSystem?.GrabFocus();
		_combatSkillSelectionItemListSystem?.Select(0);
		_combatSkillSelectionItemListSystem?.OnItemSelected(0);
	}
}