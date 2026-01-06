using Godot;
using Godot.Collections;
using PC.SlimeFactory;

namespace PC.Components.Damageable;

public partial class FlatDamageResistComponent : Component
{
    /// <summary>
    /// Flatly reduce the amount of damage taken in a category
    /// </summary>
    [Export] public Dictionary<DamageCategory, float> FlatDamageCategoryResistance = new();
    
    /// <summary>
    /// Flatly reduce the amount of damage taken in a type
    /// </summary>
    [Export] public Dictionary<DamageType, float> FlatDamageTypeResistance = new();
}