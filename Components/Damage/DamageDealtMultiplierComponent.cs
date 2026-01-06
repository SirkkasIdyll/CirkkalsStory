using Godot;
using Godot.Collections;
using PC.Components.Damageable;
using PC.SlimeFactory;

namespace PC.Components.Damage;

public partial class DamageDealtMultiplierComponent : Component
{
    /// <summary>
    /// Increase damage in a specific category
    /// </summary>
    [Export] public Dictionary<DamageCategory, float> DamageDealtCategoryMultiplier = new();

    /// <summary>
    /// Increase damage in a specific type
    /// </summary>
    [Export] public Dictionary<DamageType, float> DamageDealtTypeMultiplier = new();
}