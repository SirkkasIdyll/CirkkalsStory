using PC.Components.Interaction;

namespace PC.SlimeFactory.Signals;

public partial class SignalBus
{
    public delegate void InteractWithSignalHandler(Node<InteractableComponent> node,
        ref InteractWithSignal args);
    public event InteractWithSignalHandler? InteractWithSignal;
    public void EmitInteractWithSignal(Node<InteractableComponent> node, ref InteractWithSignal args)
    {
        InteractWithSignal?.Invoke(node, ref args);
    }

    public delegate void GetContextActionsSignalHandler(Node<InteractableComponent> node,
        ref GetContextActionsSignal args);
    public event GetContextActionsSignalHandler? GetContextActionsSignal;
    public void EmitGetContextActionsSignal(Node<InteractableComponent> node, ref GetContextActionsSignal args)
    {
        GetContextActionsSignal?.Invoke(node, ref args);
    }
    
    public delegate void ShowInteractOutlineSignalHandler(Node<CanInteractComponent> node,
        ref ShowInteractOutlineSignal args);
    public event ShowInteractOutlineSignalHandler? ShowInteractOutlineSignal;
    public void EmitShowInteractOutlineSignal(Node<CanInteractComponent> node, ref ShowInteractOutlineSignal args)
    {
        ShowInteractOutlineSignal?.Invoke(node, ref args);
    }
    
    public delegate void HideInteractOutlineSignalHandler(Node<CanInteractComponent> node,
        ref HideInteractOutlineSignal args);
    public event HideInteractOutlineSignalHandler? HideInteractOutlineSignal;
    public void EmitHideInteractOutlineSignal(Node<CanInteractComponent> node, ref HideInteractOutlineSignal args)
    {
        HideInteractOutlineSignal?.Invoke(node, ref args);
    }
}