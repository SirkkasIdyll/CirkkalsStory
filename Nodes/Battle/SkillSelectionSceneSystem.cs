using CS.Components;
using CS.Components.Description;
using CS.Components.Skills;
using CS.Nodes.Skills.Manager;
using Godot;

namespace CS.Nodes.Battle;

public partial class SkillSelectionSceneSystem : Control
{
	public CharacterBody2D? Player;
	private SkillManagerSceneSystem? _skillManagerSceneSystem;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		VisibilityChanged += OnVisibilityChanged;

		_skillManagerSceneSystem = GetNode<SkillManagerSceneSystem>("/root/MainScene/SkillManagerScene");
	}
	
	private void OnVisibilityChanged()
	{
		if (Player == null)
			return;
		
		if (_skillManagerSceneSystem == null)
			return;

		if (!ComponentSystem.TryGetComponent<SkillListComponent>(Player, out var skillListComponent))
			return;
		
		GD.Print(skillListComponent.SkillList.Count);

		if (!_skillManagerSceneSystem.TryGetSkill(skillListComponent.SkillList[0], out var skill))
			return;

		GD.Print(skill.Name);

		if (!ComponentSystem.TryGetComponent<DescriptionComponent>(skill, out var descriptionComponent))
			return;
		
		GD.Print(descriptionComponent.Description);
	}
}