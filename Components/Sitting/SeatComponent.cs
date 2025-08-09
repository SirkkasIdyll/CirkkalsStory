using CS.SlimeFactory;
using Godot;

namespace CS.Components.Sitting;

public partial class SeatComponent : Component
{
    /// <summary>
    /// Whether the seat is occupied or not
    /// </summary>
    public bool Occupied;

    /// <summary>
    /// The joint attaching the two objects together
    /// </summary>
    public PinJoint2D? PinJoint2D;
}