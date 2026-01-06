using Godot;
using PC.Nodes.UI.Chyron;
using PC.SlimeFactory;
using PC.SlimeFactory.Signals;

namespace PC.Components.Interaction;

public partial class CanInteractComponent : Component
{
    /// <summary>
    /// The max distance the node can be to successfully interact with objects
    /// </summary>
    [Export]
    public float MaxInteractDistance = 2.25f;
    
    /// <summary>
    /// The target to potentially interact with
    /// </summary>
    public Node? InteractTarget;
    
    /// <summary>
    /// TODO: Make this Chyron appear below the target being highlighted?, makes a bit more sense
    /// 
    /// Shows what is being highlighted to interact with
    /// </summary>
    public Chyron? Chyron;
    
    /// <summary>
    /// Time in seconds before the Chyron appears
    /// </summary>
    public float TimeBeforeChyron = 0.25f;

}

/// <summary>
/// Registers the interact target in the <see cref="CanInteractComponent"/> and updates the outline
/// based on this node's range to the target
/// </summary>
public partial class ShowInteractOutlineSignal : UserSignalArgs
{
    public Node InteractTarget;

    public ShowInteractOutlineSignal(Node interactTarget)
    {
        InteractTarget = interactTarget;
    }
}

/// <summary>
/// Unregisters the interact target from the <see cref="CanInteractComponent"/> and hides the outline
/// </summary>
public partial class HideInteractOutlineSignal : UserSignalArgs
{
    public Node InteractTarget;

    public HideInteractOutlineSignal(Node interactTarget)
    {
        InteractTarget = interactTarget;
    }
}