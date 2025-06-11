using Godot;

namespace CS.Scripts.Components.Damage;

public partial class DamageOverTimeComponent : Node2D
{
    [Export] public int Damage;
    [Export] public int Turns;
    
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
}