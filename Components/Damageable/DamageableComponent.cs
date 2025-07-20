using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Components.Damageable;

/// <summary>
/// Required for a mob to be affected by any form of damage
/// </summary>
public partial class DamageableComponent : Component
{
    /// <summary>
    /// Setting a category to 0f will grant immunity
    /// 1f = 100% damage taken in a category, 2f = 200%, 0.5f = 50%
    /// </summary>
    [Export] public Dictionary<DamageCategory, float> DamageCategoryResistance = new()
    {
        { DamageCategory.Physical , 1f},
        { DamageCategory.Magical , 1f}
    };
    
    /// <summary>
    /// Setting a category to 0f will grant immunity
    /// 1f = 100% damage taken in a category, 2f = 200%, 0.5f = 50%
    /// </summary>
    [Export] public Dictionary<DamageType, float> DamageTypeResistance = new()
    {
        { DamageType.Blunt , 1f},
        { DamageType.Slash , 1f},
        { DamageType.Piercing , 1f},
        { DamageType.Fire , 1f},
        { DamageType.Water , 1f},
        { DamageType.Ice , 1f},
        { DamageType.Poison , 1f},
        { DamageType.Internal, 1f}
    };
}

/// <summary>
/// When adding a new DamageCategory, add it to the resistance dictionary
/// </summary>
public enum DamageCategory
{
    Physical,
    Magical
}

/// <summary>
/// When adding a new DamageType, add it to the resistance dictionary. Otherwise nothing takes that form of damage.
/// </summary>
public enum DamageType
{
    Blunt,
    Slash,
    Piercing,
    Fire,
    Water,
    Ice,
    Poison,
    Internal
}