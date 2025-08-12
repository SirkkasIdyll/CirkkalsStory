using System.Diagnostics.CodeAnalysis;
using CS.Components.Mob;
using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Components.Ability;

public partial class AbilitySystem : NodeSystem
{
    public Dictionary<string, Node> AbilityDictionary = [];

    public override void _Ready()
    {
        base._Ready();
        
        LoadDictionary();
    }
    
    /// <summary>
    /// Fetches an array of the known abilities
    /// </summary>
    /// <param name="node">A mob</param>
    /// <param name="abilities">Skills from the <see cref="MobComponent"/></param>
    public void GetKnownSkills(Node node, out Array<string> abilities)
    {
        abilities = [];

        if (!_nodeManager.TryGetComponent<MobComponent>(node, out var mobComponent))
            return;

        abilities = mobComponent.Abilities;
    }

    /// <summary>
    /// Loads all skills into the ability repository.
    /// Skill details can be retrieved from the ability repository.
    /// </summary>
    private void LoadDictionary()
    {
        foreach (var node in _nodeManager.NodeDictionary.Values)
        {
            if (!_nodeManager.HasComponent<AbilityComponent>(node))
                continue;
            
            AbilityDictionary.Add(node.Name, node);
        }
    }
    
    /// <summary>
    /// Attempts to return the ability node if it exists in the ability repository
    /// </summary>
    /// <param name="name">The name of the ability to retrieve</param>
    /// <param name="ability">The node containing the ability and all its child components</param>
    /// <returns>True if ability found, false if ability not found</returns>
    public bool TryGetAbility(string name, [NotNullWhen(true)] out Node? ability)
    {
        return AbilityDictionary.TryGetValue(name, out ability);
    }

    /// <summary>
    /// Checks if the ability exists in the repository
    /// </summary>
    /// <param name="name">The name of the ability</param>
    /// <returns>True if ability found, false if ability not found</returns>
    public bool AbilityExists(string name)
    {
        return AbilityDictionary.ContainsKey(name);
    }
}