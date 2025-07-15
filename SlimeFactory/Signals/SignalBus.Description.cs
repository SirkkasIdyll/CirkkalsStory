using CS.Components.Description;

namespace CS.SlimeFactory.Signals;

public partial class SignalBus
{
    public delegate void ReloadCombatDescriptionSignalHandler(Node<DescriptionComponent> node, ref ReloadCombatDescriptionSignal args);
    public event ReloadCombatDescriptionSignalHandler? ReloadCombatDescriptionSignal;
    public void EmitReloadCombatDescriptionSignal(Node<DescriptionComponent> node, ref ReloadCombatDescriptionSignal args)
    {
        ReloadCombatDescriptionSignal?.Invoke(node, ref args);
    }

    public delegate void UpdateDisplayNameSignalHandler(Node<DescriptionComponent> node,
        ref UpdateDisplayNameSignal args);
    public event UpdateDisplayNameSignalHandler? UpdateDisplayNameSignal;
    public void EmitUpdateDisplayNameSignal(Node<DescriptionComponent> node, ref UpdateDisplayNameSignal args)
    {
        UpdateDisplayNameSignal?.Invoke(node, ref args);
    }
}