using Godot;
using PC.SlimeFactory;

namespace PC.Components.Damageable;

/// <summary>
/// How much health it costs to use a certain spell or skill
/// </summary>
public partial class HealthCostComponent : Component
{
    [Export] public int HealthCost;
}