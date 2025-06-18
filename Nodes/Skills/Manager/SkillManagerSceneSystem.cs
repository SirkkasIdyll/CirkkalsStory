using System.Diagnostics.CodeAnalysis;
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
    
	/// <summary>
	/// Loads all skills into the skill repository. Skill details can be retrieved from the skill repository.
	/// </summary>
	private void LoadAllSkills()
	{
		var children = GetChildren();
		foreach (var child in children)
		{
			if (!ComponentSystem.HasComponent<SkillComponent>(child))
				continue;
			
			if (ComponentSystem.TryGetComponent<DescriptionComponent>(child, out var descriptionComponent))
				_skillRepository.Add(descriptionComponent.DisplayName, child);
		}
	}

	/// <summary>
	/// Attempts to return the skill node if it exists in the skill repository
	/// </summary>
	/// <param name="name">The name of the skill to retrieve</param>
	/// <param name="skill">The node containing the skill and all its child components</param>
	/// <returns>True if skill found, false if skill not found</returns>
	public bool TryGetSkill(string name, [NotNullWhen(true)] out Node? skill)
	{
		return _skillRepository.TryGetValue(name, out skill);
	}

	/// <summary>
	/// Checks if the skill exists in the repository
	/// </summary>
	/// <param name="name">The name of the skill</param>
	/// <returns>True if skill found, false if skill not found</returns>
	public bool SkillExists(string name)
	{
		return _skillRepository.ContainsKey(name);
	}
}