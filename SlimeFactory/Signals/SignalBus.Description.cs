using PC.Components.Description;

namespace PC.SlimeFactory.Signals;

public partial class SignalBus
{
    public delegate void GetActionEffectsDescriptionSignalHandler(Node<DescriptionComponent> node, ref GetActionEffectsDescriptionSignal args);
    public event GetActionEffectsDescriptionSignalHandler? GetActionEffectsDescriptionSignal;
    public void EmitGetActionEffectsDescriptionSignal(Node<DescriptionComponent> node, ref GetActionEffectsDescriptionSignal args)
    {
        GetActionEffectsDescriptionSignal?.Invoke(node, ref args);
    }
    
    public delegate void GetActionCostsDescriptionSignalHandler(Node<DescriptionComponent> node, ref GetActionCostsDescriptionSignal args);
    public event GetActionCostsDescriptionSignalHandler? GetActionCostsDescriptionSignal;
    public void EmitGetActionCostsDescriptionSignal(Node<DescriptionComponent> node, ref GetActionCostsDescriptionSignal args)
    {
        GetActionCostsDescriptionSignal?.Invoke(node, ref args);
    }

    public delegate void UpdateDisplayNameSignalHandler(Node<DescriptionComponent> node,
        ref UpdateDisplayNameSignal args);
    public event UpdateDisplayNameSignalHandler? UpdateDisplayNameSignal;
    public void EmitUpdateDisplayNameSignal(Node<DescriptionComponent> node, ref UpdateDisplayNameSignal args)
    {
        UpdateDisplayNameSignal?.Invoke(node, ref args);
    }
}