using CS.Components.Magic;

namespace CS.SlimeFactory.Signals;

public partial class SignalBus
{
    public delegate void CheckSpellCastableSignalHandler(Node<SpellComponent> node, ref CheckSpellCastableSignal args);
    public event CheckSpellCastableSignalHandler? CheckSpellCastableSignal;
    public void EmitCheckSpellCastableSignal(Node<SpellComponent> node, ref CheckSpellCastableSignal args)
    {
        CheckSpellCastableSignal?.Invoke(node, ref args);
    }
    
    public delegate void CastSpellSignalHandler(Node<SpellComponent> node, ref CastSpellSignal args);
    public event CastSpellSignalHandler? CastSpellSignal;
    public void EmitCastSpellSignal(Node<SpellComponent> node, ref CastSpellSignal args)
    {
        CastSpellSignal?.Invoke(node, ref args);
    }

    public delegate void ManaAlteredSignalHandler(Node<ManaComponent> node, ref ManaAlteredSignal args);
    public event ManaAlteredSignalHandler? ManaAlteredSignal;
    public void EmitManaAlteredSignal(Node<ManaComponent> node, ref ManaAlteredSignal args)
    {
        ManaAlteredSignal?.Invoke(node, ref args);
    }
}