using CS.Components.Damageable;
using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Components.Damage;

public partial class FlatDamageIncreaseComponent : Component
{
    /// <summary>
    /// Flatly increase damage dealt in category
    /// </summary>
    [Export] public Dictionary<DamageCategory, float> FlatDamageCategoryIncrease = new();
    
    /// <summary>
    /// Flatly increase damage dealt in type
    /// </summary>
    [Export] public Dictionary<DamageType, float> FlatDamageTypeIncrease = new();
}