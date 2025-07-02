using System;
using CS.SlimeFactory;
using Godot;

namespace CS.Components.Damageable;

public partial class HealthComponent : Component
{
    /// <summary>
    /// Positive changes indicate healing, negative changes indicate damage
    /// </summary>
    [Signal]
    public delegate void HealthAlteredEventHandler(int amount, int result);
    
    private int _healthCap = 9999;
    private int _maxHealth;
    private int _health;
    
    /// <summary>
    /// Cannot exceed <see cref="_healthCap"/>
    /// </summary>
    [Export(PropertyHint.Range, "0,9999")]
    public int MaxHealth
    {
        get => _maxHealth;
        set => _maxHealth = Math.Clamp(value, 0, _healthCap);
    }

    /// <summary>
    /// Cannot exceed <see cref="MaxHealth"/>
    /// To make damageable, add the <see cref="DamageableComponent"/>
    /// </summary>
    [Export(PropertyHint.Range, "0,9999")]
    public int Health
    {
        get => _health;
        set => _health = Math.Clamp(value, 0, _maxHealth);
    }

    public void AlterHealth(int amount)
    {
        Health += amount;
        EmitSignalHealthAltered(amount, Health);
    }
}