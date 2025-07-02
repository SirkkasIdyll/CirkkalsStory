using CS.SlimeFactory;
using Godot;

namespace CS.Components.Magic;

/// <summary>
/// How much mana it costs to use a certain spell or skill
/// </summary>
public partial class ManaCostComponent : Component
{
    [Export] public int ManaCost;
}