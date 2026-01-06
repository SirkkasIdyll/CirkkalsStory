using Godot;
using Godot.Collections;
using PC.SlimeFactory;

namespace PC.Components.Mob;

/// <summary>
/// Mobs are the general term for all combat-capable creatures.
/// This component stores their abilities, skills, and spells in one convenient location.
/// </summary>
public partial class MobComponent : Component
{
    private const string SerializeAttributeName = "Serialize";
    
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
    public Dictionary<string, Node> StatusEffects = [];
}