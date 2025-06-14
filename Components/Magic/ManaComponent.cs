using System;
using Godot;

namespace CS.Components.Magic;

public partial class ManaComponent : Node2D
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