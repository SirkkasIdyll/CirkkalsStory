using System;
using CS.SlimeFactory;
using CS.SlimeFactory.Signals;
using Godot;

namespace CS.Components.Magic;

public partial class ManaComponent : Component
{
    private int _manaCap = 9999;
    private int _maxMana;
    private int _mana;

    [Export(PropertyHint.Range, "0,9999")]
    public int MaxMana
    {
        get => _maxMana;
        set => _maxMana = Math.Clamp(value, 0, _manaCap);
    }

    /// <summary>
    /// Cannot exceed max mana
    /// </summary>
    [Export(PropertyHint.Range, "0,9999")]
    public int Mana
    {
        get => _mana;
        set => _mana = Math.Clamp(value, 0, _maxMana);
    }
}

public partial class PreviewManaAlteredSignal : UserSignalArgs
{
    public int Amount;

    public PreviewManaAlteredSignal(int amount)
    {
        Amount = amount;
    }
}

public partial class ManaAlteredSignal : UserSignalArgs
{
    
}