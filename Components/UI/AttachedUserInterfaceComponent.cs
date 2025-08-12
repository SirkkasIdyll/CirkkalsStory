using CS.SlimeFactory;
using Godot;

namespace CS.Components.UI;

public partial class AttachedUserInterfaceComponent : Component
{
    /// <summary>
    /// The UserInterface to open
    /// </summary>
    [Export]
    public PackedScene UserInterfaceScene = null!;
    
    /// <summary>
    /// Closes the UI if the user gets farther than this distance to the node
    /// </summary>
    [Export]
    public float MaxUseDistance = 2.25f;
    
    public Control? UserInterface;
    public Node? User;
}