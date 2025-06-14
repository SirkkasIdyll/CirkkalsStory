using Godot;

namespace CS.Components.Damageable;

/// <summary>
/// How much health it costs to use a certain spell or skill
/// </summary>
public partial class HealthCostComponent : Node2D
{
    [Export] public int HealthCost;
}