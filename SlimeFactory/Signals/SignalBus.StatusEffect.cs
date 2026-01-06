using PC.Components.StatusEffect;

namespace PC.SlimeFactory.Signals;

public partial class SignalBus
{
    public delegate void ProcStatusEffectSignalHandler(Node<StatusEffectComponent> node,
        ref ProcStatusEffectSignal args);
    public event ProcStatusEffectSignalHandler? ProcStatusEffectSignal;
    public void EmitProcStatusEffectSignal(Node<StatusEffectComponent> node, ref ProcStatusEffectSignal args)
    {
        ProcStatusEffectSignal?.Invoke(node, ref args);
    }
}