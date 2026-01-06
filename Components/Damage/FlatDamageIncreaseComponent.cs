using Godot;
using Godot.Collections;
using PC.Components.Damageable;
using PC.SlimeFactory;

namespace PC.Components.Damage;

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