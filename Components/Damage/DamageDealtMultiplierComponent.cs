using CS.Components.Damageable;
using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Components.Damage;

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