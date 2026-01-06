using System.Collections.Generic;
using Godot;
using PC.SlimeFactory;

namespace PC.Components.UI;

public partial class AttachedUserInterfaceComponent : Component
{
    /// <summary>
    /// The string is generally the action used to open the user interface, or the name of the UI
    /// The PackedScene is the interface to open
    /// </summary>
    [Export]
    public Godot.Collections.Dictionary<string, PackedScene> UserInterfaceScenes = [];
    
    /// <summary>
    /// Closes the UI if the user gets farther than this distance to the node
    /// </summary>
    [Export]
    public float MaxUseDistance = 2.25f;

    /// <summary>
    /// When set to false, prevents more than one user from interacting with the interfaces from this scene at a time
    /// </summary>
    [Export]
    public bool AllowSimultaneousUse = true;
    
    /// <summary>
    /// Tracks the active user interfaces open and who is viewing which specific interface
    /// </summary>
    public Dictionary<string, Dictionary<Node, Control>> ActiveUserInterfaces = [];
}