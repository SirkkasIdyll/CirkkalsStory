using Godot;
using PC.Components.Damageable;
using PC.SlimeFactory;

namespace PC.Components.Damage;

/// <summary>
/// Dictates the amount of damage a node is capable of doing
/// </summary>
public partial class DamageComponent : Component
{
    /// <summary>
    /// The amount of damage dealt
    /// </summary>
    [Export] public float Damage;

    /// <summary>
    /// Physical or Magical
    /// </summary>
    [Export] public DamageCategory DamageCategory;
    
    /// <summary>
    /// More specific damage types
    /// </summary>
    [Export] public DamageType DamageType;
}