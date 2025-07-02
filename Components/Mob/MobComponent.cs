using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Components.Mob;

/// <summary>
/// Mobs are the general term for all combat-capable creatures.
/// This component stores their abilities, skills, and spells in one convenient location.
/// </summary>
public partial class MobComponent : Component
{
    /// <summary>
    /// Abilities are passive effects that each mob has. 
    /// </summary>
    [Export] public Array<string> Abilities = [];
    
    /// <summary>
    /// Skills are physical abilities a mob can use during combat
    /// </summary>
    [Export] public Array<string> Skills = [];
    
    /// <summary>
    /// Spells are magical abilities a mob can use during combat
    /// </summary>
    [Export] public Array<string> Spells = [];

    /// <summary>
    /// Status effects can be positive or negative
    /// </summary>
    [Export] public Array<Node> StatusEffects = [];

    public override void _Ready()
    {
        base._Ready();
        
    }

    public string? ChooseRandomSkillOrSpell()
    {
        if (Skills.Count == 0)
            return null;
        
        return Skills.PickRandom();
        
        
    }
}