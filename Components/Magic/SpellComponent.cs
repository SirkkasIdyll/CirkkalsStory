using CS.SlimeFactory;
using CS.SlimeFactory.Signals;
using Godot;

namespace CS.Components.Magic;

/// <summary>
/// Spell nodes will show up in a mob's spell options
/// </summary>
public partial class SpellComponent : Component
{
}

public partial class CheckSpellCastableSignal : UserSignalArgs
{
    public Node Spellcaster;
    public bool Castable;

    public CheckSpellCastableSignal(Node spellcaster)
    {
        Spellcaster = spellcaster;
    }
}

public partial class CastSpellSignal : UserSignalArgs
{
    public Node Spellcaster;

    public CastSpellSignal(Node spellcaster)
    {
        Spellcaster = spellcaster;
    }
}