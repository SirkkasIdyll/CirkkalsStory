using Godot;
using PC.SlimeFactory;

namespace PC.Components.Magic;

/// <summary>
/// How much mana it costs to use a certain spell or skill
/// </summary>
public partial class ManaCostComponent : Component
{
    [Export] public int ManaCost;
}