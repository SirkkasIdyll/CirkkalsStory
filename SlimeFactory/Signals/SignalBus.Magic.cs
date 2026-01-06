using PC.Components.Magic;

namespace PC.SlimeFactory.Signals;

public partial class SignalBus
{
    public delegate void CheckSpellCastableSignalHandler(Node<SpellComponent> node, ref CheckSpellCastableSignal args);
    public event CheckSpellCastableSignalHandler? CheckSpellCastableSignal;
    public void EmitCheckSpellCastableSignal(Node<SpellComponent> node, ref CheckSpellCastableSignal args)
    {
        CheckSpellCastableSignal?.Invoke(node, ref args);
    }
    
    public delegate void PreviewManaAlteredSignalHandler(Node<ManaComponent> node, ref PreviewManaAlteredSignal args);
    public event PreviewManaAlteredSignalHandler? PreviewManaAlteredSignal;
    public void EmitPreviewManaAlteredSignal(Node<ManaComponent> node, ref PreviewManaAlteredSignal args)
    {
        PreviewManaAlteredSignal?.Invoke(node, ref args);
    }

    public delegate void ManaAlteredSignalHandler(Node<ManaComponent> node, ref ManaAlteredSignal args);
    public event ManaAlteredSignalHandler? ManaAlteredSignal;
    public void EmitManaAlteredSignal(Node<ManaComponent> node, ref ManaAlteredSignal args)
    {
        ManaAlteredSignal?.Invoke(node, ref args);
    }
}