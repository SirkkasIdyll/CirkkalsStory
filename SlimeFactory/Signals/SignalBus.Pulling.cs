using CS.Components.Interaction;
using CS.Components.Pulling;

namespace CS.SlimeFactory.Signals;

public partial class SignalBus
{
    public delegate void PullActionSignalHandler(Node<CanInteractComponent> node, ref PullActionSignal args);
    public event PullActionSignalHandler? PullActionSignal;
    public void EmitPullActionSignal(Node<CanInteractComponent> node, ref PullActionSignal args)
    {
        PullActionSignal?.Invoke(node, ref args);
    }
}