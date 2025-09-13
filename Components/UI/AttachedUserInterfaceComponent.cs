using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Components.UI;

public partial class AttachedUserInterfaceComponent : Component
{
    /// <summary>
    /// The string is generally the action used to open the user interface, or the name of the UI
    /// The PackedScene is the interface to open
    /// </summary>
    [Export]
    public Dictionary<string, PackedScene> UserInterfaceScenes = [];
    
    /// <summary>
    /// Closes the UI if the user gets farther than this distance to the node
    /// </summary>
    [Export]
    public float MaxUseDistance = 2.25f;
    
    public Dictionary<string, Control> UserInterface = [];
    public Dictionary<string, Node> UserUsingInterface = [];
}