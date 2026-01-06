using PC.Components.Pulling;

namespace PC.SlimeFactory.Signals;

public partial class SignalBus
{
    public delegate void CanPullSignalHandler(Node<CanPullThingsComponent> node,
        ref CanPullSignal args);
    public event CanPullSignalHandler? CanPullSignal;
    public void EmitCanPullSignal(Node<CanPullThingsComponent> node, ref CanPullSignal args)
    {
        CanPullSignal?.Invoke(node, ref args);
    }

    public delegate void CanStopPullingSignalHandler(Node<CanPullThingsComponent> node, ref CanStopPullingSignal args);
    public event CanStopPullingSignalHandler? CanStopPulling;
    public void EmitCanStopPullingSignal(Node<CanPullThingsComponent> node, ref CanStopPullingSignal args)
    {
        CanStopPulling?.Invoke(node, ref args);
    }
}