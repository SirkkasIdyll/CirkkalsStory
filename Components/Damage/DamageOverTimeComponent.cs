using Godot;

namespace CS.Components.Damage;

/// <summary>
/// Dictates the amount of damage over time a node will do,
/// and whether the damage is capable of stacking in damage and/or duration.
/// </summary>
public partial class DamageOverTimeComponent : Node2D
{
    /// <summary>
    /// The amount of damage that will occur each turn
    /// </summary>
    [Export] public int Damage;
    
    /// <summary>
    /// When true, stacks damage on existing damage when applied again
    /// Otherwise damage remains the same on reapplication
    /// </summary>
    [Export] public bool StacksDamage = false;

    /// <summary>
    /// When true, stacks additional turns when applied again
    /// Otherwise, turns simply get refreshed
    /// </summary>
    [Export] public bool StacksDuration = false;
    
    /// <summary>
    /// How many more times the damage will occur on the target before the start of their turn
    /// </summary>
    [Export] public int Turns;
}