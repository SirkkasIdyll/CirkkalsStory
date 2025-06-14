using Godot;

namespace CS.Components.Description;

/// <summary>
/// Used for things that need to be described such as skills, spells, or monsters
/// </summary>
public partial class DescriptionComponent : Node2D
{
    /// <summary>
    /// Roxxaannnee....
    /// </summary>
    [Export] public string DisplayName = "Roxanne";

    /// <summary>
    /// Systems should aim to add onto the description rather than completely overwriting it.
    /// This allows multiple different systems to add their own descriptions.
    /// </summary>
    [Export] public string Description = "";
}