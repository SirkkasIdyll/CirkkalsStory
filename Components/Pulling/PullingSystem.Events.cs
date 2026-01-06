using PC.SlimeFactory;
using PC.SlimeFactory.Signals;

namespace PC.Components.Pulling;

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