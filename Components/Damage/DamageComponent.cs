using CS.Components.Damageable;
using Godot;

namespace CS.Components.Damage;

/// <summary>
/// Dictates the amount of damage a node is capable of doing
/// </summary>
public partial class DamageComponent : Node2D
{
    /// <summary>
    /// The amount of damage dealt
    /// </summary>
    [Export] public int Damage;

    public void ApplyCombatEffect(Node target)
    {
        if (ComponentSystem.TryGetComponent<HealthComponent>(target, out var targetHealthComponent))
            targetHealthComponent.AlterHealth(-Damage);
    }

    public string DescribeEffect()
    {
        return $"Damage: {Damage}";
    }
}