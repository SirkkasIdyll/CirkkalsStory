using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Components.Damageable;

public partial class DamageResistMultiplierComponent : Component
{
    /// <summary>
    /// 1f = 100% damage taken in a category
    /// 2f = 200% resist (50% damage taken)
    /// 3f = 300% resist (1 / 3, 33% damage taken)
    /// </summary>
    [Export] public Dictionary<DamageCategory, float> DamageCategoryResistanceMultiplier = new();
    
    /// <summary>
    /// 1f = 100% damage taken in a category
    /// 2f = 200% resist (50% damage taken)
    /// 3f = 300% resist (1 / 3, 33% damage taken)
    /// </summary>
    [Export] public Dictionary<DamageType, float> DamageTypeResistanceMultiplier = new();
}