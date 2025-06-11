using Godot;

namespace CS.Scripts.Components.Magic;

/// <summary>
/// How much mana it costs to use a certain spell or skill
/// </summary>
public partial class ManaCostComponent : Node2D
{
    [Export] public int ManaCost;
}