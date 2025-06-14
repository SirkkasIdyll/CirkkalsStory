using Godot;

namespace CS.Components.Damage;

/// <summary>
/// Dictates the amount of damage a node is capable of doing
/// </summary>
public partial class DamageComponent : Node2D
{
    /// <summary>
    /// The amount of damage dealt
    /// </summary>
    [Export] public int Damage;
}