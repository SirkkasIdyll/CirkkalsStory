using PC.Components.Movement;

namespace PC.SlimeFactory.Signals;

public partial class SignalBus
{
    public delegate void MovementAttemptSignalHandler(Node<MovementComponent> node, ref MovementAttemptSignal args);
    public event MovementAttemptSignalHandler? MovementAttemptSignal;
    public void EmitMovementAttemptSignal(Node<MovementComponent> node, ref MovementAttemptSignal args)
    {
        MovementAttemptSignal?.Invoke(node, ref args);
    }
    
    public delegate void MovementSignalHandler(Node<MovementComponent> node, ref MovementSignal args);
    public event MovementSignalHandler? MovementSignal;
    public void EmitMovementSignal(Node<MovementComponent> node, ref MovementSignal args)
    {
        MovementSignal?.Invoke(node, ref args);
    }
}