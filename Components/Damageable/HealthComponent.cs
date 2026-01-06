using System;
using Godot;
using PC.SlimeFactory;
using PC.SlimeFactory.Signals;

namespace PC.Components.Damageable;

public partial class HealthComponent : Component
{
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
}

public partial class PreviewHealthAlteredSignal : UserSignalArgs
{
    public int Amount;

    public PreviewHealthAlteredSignal(int amount)
    {
        Amount = amount;
    }
}

public partial class HealthAlteredSignal : UserSignalArgs
{
    
}

public partial class MobDiedSignal : UserSignalArgs
{
    
}