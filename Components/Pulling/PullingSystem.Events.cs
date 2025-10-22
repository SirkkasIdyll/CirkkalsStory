using CS.SlimeFactory;
using CS.SlimeFactory.Signals;

namespace CS.Components.Pulling;

public partial class CanPullSignal : CancellableSignalArgs
{
    public Node<PullableComponent> Pullable;

    public CanPullSignal(Node<PullableComponent> pullable)
    {
        Pullable = pullable;
    }
}

public partial class CanStopPullingSignal : CancellableSignalArgs
{
    public Node<PullableComponent> Pullable;

    public CanStopPullingSignal(Node<PullableComponent> pullable)
    {
        Pullable = pullable;
    }
}