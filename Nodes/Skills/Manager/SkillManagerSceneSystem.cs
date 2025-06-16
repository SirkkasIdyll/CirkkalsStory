using CS.Components;
using CS.Components.Description;
using CS.Components.Skills;
using Godot;
using Godot.Collections;

namespace CS.Nodes.Skills.Manager;

public partial class SkillManagerSceneSystem : Node2D
{
	private Dictionary<string, Node> _skillRepository = [];

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		LoadAllSkills();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
    
	private void LoadAllSkills()
	{
		var children = GetChildren();
		foreach (var child in children)
		{
			if (!ComponentSystem.HasComponent<SkillComponent>(child))
				continue;
			
			if (ComponentSystem.GetComponent<DescriptionComponent>(child, out var descriptionComponent) && descriptionComponent != null)
				_skillRepository.Add(descriptionComponent.DisplayName, child);
		}
	}
    
	public void AddSkill(string name)
	{
	}
}