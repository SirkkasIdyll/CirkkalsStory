using Godot;

namespace CS.Scripts.Components.Description;

/// <summary>
/// Used for things that need to be described such as skills, spells, or monsters
/// </summary>
public partial class DescriptionComponent : Node2D
{
    [Export] public string Description;
}