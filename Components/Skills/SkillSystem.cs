using System.Diagnostics.CodeAnalysis;
using Godot;
using Godot.Collections;
using PC.Components.Mob;
using PC.SlimeFactory;

namespace PC.Components.Skills;

public partial class SkillSystem : NodeSystem
{
    private Dictionary<string, Node> _skillDictionary = [];

    public override void _Ready()
    {
        base._Ready();
        
        LoadDictionary();
    }
    
    /// <summary>
    /// Fetches an array of the known skills
    /// </summary>
    /// <param name="node">A mob</param>
    /// <param name="skills">Skills from the <see cref="MobComponent"/></param>
    public void GetKnownSkills(Node node, out Array<string> skills)
    {
        skills = [];

        if (!_nodeManager.TryGetComponent<MobComponent>(node, out var mobComponent))
            return;

        skills = mobComponent.Skills;
    }

    /// <summary>
    /// Loads all skills into the skill repository.
    /// Skill details can be retrieved from the skill repository.
    /// </summary>
    private void LoadDictionary()
    {
        foreach (var node in _nodeManager.NodeDictionary.Values)
        {
            if (!_nodeManager.HasComponent<SkillComponent>(node))
                continue;
            
            _skillDictionary.Add(node.Name, node);
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
        return _skillDictionary.TryGetValue(name, out skill);
    }

    /// <summary>
    /// Checks if the skill exists in the repository
    /// </summary>
    /// <param name="name">The name of the skill</param>
    /// <returns>True if skill found, false if skill not found</returns>
    public bool SkillExists(string name)
    {
        return _skillDictionary.ContainsKey(name);
    }
}