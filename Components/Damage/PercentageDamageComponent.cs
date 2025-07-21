using CS.Components.Damageable;
using CS.SlimeFactory;
using Godot;

namespace CS.Components.Damage;

public partial class PercentageDamageComponent : Component
{
    /// <summary>
    /// The percent amount of damage based on max health inflicted on hit
    /// </summary>
    [Export(PropertyHint.Range, "0, 100")] public float PercentDamage;

    /// <summary>
    /// Physical or Magical
    /// </summary>
    [Export] public DamageCategory DamageCategory;
    
    /// <summary>
    /// More specific damage types
    /// </summary>
    [Export] public DamageType DamageType;
}