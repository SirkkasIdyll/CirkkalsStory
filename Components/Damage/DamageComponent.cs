using CS.Components.Damageable;
using CS.SlimeFactory;
using Godot;

namespace CS.Components.Damage;

/// <summary>
/// Dictates the amount of damage a node is capable of doing
/// </summary>
public partial class DamageComponent : Component
{
    /// <summary>
    /// The amount of damage dealt
    /// </summary>
    [Export] public int Damage;

    public void ApplyCombatEffect(Node target)
    {
        if (NodeManager.Instance.TryGetComponent<HealthComponent>(target, out var targetHealthComponent))
            targetHealthComponent.AlterHealth(-Damage);
    }

    public string DescribeEffect()
    {
        return $"Damage: {Damage}";
    }
}