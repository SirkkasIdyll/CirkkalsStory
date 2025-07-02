using System.Diagnostics.CodeAnalysis;
using CS.Components.Description;
using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Components.Skills;

public partial class SkillManagerSystem : NodeSystem
{
    private Dictionary<string, Node> _skillRepository = [];
    
    public override void _SystemReady()
    {
        base._SystemReady();
        
        GetAllSkills();
        GD.Print(_skillRepository.Count);
    }

    /// <summary>
    /// Loads all skills into the skill repository.
    /// Skill details can be retrieved from the skill repository.
    /// </summary>
    private void GetAllSkills()
    {
        foreach (var nodeCompDic in _nodeManager.NodeCompHashset)
        {
            foreach (var value in nodeCompDic.Values)
            {
                if (value is SkillComponent skillComponent)
                    _skillRepository.Add(skillComponent.ParentNode.Name, skillComponent.ParentNode);
            }
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