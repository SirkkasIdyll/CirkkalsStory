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
    private const string SerializeAttributeName = "Serialize";
    
    /// <summary>
    /// Abilities are passive effects that each mob has. 
    /// </summary>
    [Export] [Serialize] public Array<string> Abilities = [];
    
    /// <summary>
    /// Skills are physical abilities a mob can use during combat
    /// </summary>
    [Export] [Serialize]  public Array<string> Skills = [];
    
    /// <summary>
    /// Spells are magical abilities a mob can use during combat
    /// </summary>
    [Export] [Serialize]  public Array<string> Spells = [];

    /// <summary>
    /// Status effects can be positive or negative
    /// </summary>
    [Serialize] public Dictionary<string, Node> StatusEffects = [];
}