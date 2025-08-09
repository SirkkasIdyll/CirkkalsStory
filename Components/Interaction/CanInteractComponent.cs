using CS.SlimeFactory;
using CS.SlimeFactory.Signals;
using Godot;

namespace CS.Components.Interaction;

public partial class CanInteractComponent : Component
{
    /// <summary>
    /// The max distance the node can be to successfully interact with objects
    /// </summary>
    [Export]
    public float MaxInteractDistance = 75f;
    
    /// <summary>
    /// The target to potentially interact with
    /// </summary>
    public Node? InteractTarget;
}

public partial class ShowInteractOutlineSignal : UserSignalArgs
{
    public Node InteractTarget;

    public ShowInteractOutlineSignal(Node interactTarget)
    {
        InteractTarget = interactTarget;
    }
}


public partial class HideInteractOutlineSignal : UserSignalArgs
{
    public Node InteractTarget;

    public HideInteractOutlineSignal(Node interactTarget)
    {
        InteractTarget = interactTarget;
    }
}