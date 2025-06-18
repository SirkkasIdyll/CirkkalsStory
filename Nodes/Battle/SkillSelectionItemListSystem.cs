using CS.Components;
using CS.Components.Description;
using CS.Nodes.Skills.Manager;
using Godot;
using Godot.Collections;

namespace CS.Nodes.Battle;

public partial class SkillSelectionItemListSystem : ItemList
{
	private SkillManagerSceneSystem? _skillManagerSceneSystem;
	[Export] private SkillSelectionSceneSystem? _skillSelectionSceneSystem;
	[Export] private Label? _skillName;
	[Export] private Label? _skillEffect;
	[Export] private Label? _skillEffect2;
	[Export] private Label? _skillDescription;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ItemSelected += OnItemSelected;
		
		if (_skillSelectionSceneSystem != null)
			_skillSelectionSceneSystem.DisplayPlayerSkills += OnDisplayPlayerSkills;
		
		// TODO: Un-hardcode SkillManager
		_skillManagerSceneSystem = GetNode<SkillManagerSceneSystem>("/root/MainScene/SkillManagerScene");
	}

	private void OnItemSelected(long index)
	{
		if (_skillManagerSceneSystem == null)
			return;

		if (!_skillManagerSceneSystem.TryGetSkill(GetItemText((int)index), out var skill))
			return;

		if (!ComponentSystem.TryGetComponent<DescriptionComponent>(skill, out var descriptionComponent))
			return;

		if (_skillName != null)
			_skillName.Text = descriptionComponent.DisplayName;

		if (_skillDescription != null)
			_skillDescription.Text = descriptionComponent.Description;
	}

	private void OnDisplayPlayerSkills(Array<string> skills)
	{
		Clear();
		_skillName.Text = "";
		_skillEffect.Text = "";
		_skillEffect2.Text = "";
		_skillDescription.Text = "";
		
		if (_skillManagerSceneSystem == null)
			return;
		
		// Prevents adding invalid skills to battle options
		foreach (var skill in skills)
		{
			if (_skillManagerSceneSystem.SkillExists(skill))
				AddItem(skill);
		}
		
		GrabFocus();
	}
}