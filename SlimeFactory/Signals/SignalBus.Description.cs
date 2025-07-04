using CS.Components.Description;

namespace CS.SlimeFactory.Signals;

public partial class SignalBus
{
    public delegate void GetDescriptionSignalHandler(Node<DescriptionComponent> node, ref GetDescriptionSignal args);
    public event GetDescriptionSignalHandler? GetDescriptionSignal;
    public void EmitGetDescriptionSignal(Node<DescriptionComponent> node, ref GetDescriptionSignal args)
    {
        GetDescriptionSignal?.Invoke(node, ref args);
    }
}