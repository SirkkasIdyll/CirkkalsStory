using Godot;

namespace CS.Scripts.Components.Description;

/// <summary>
/// Every entity with a name should have one of these!
/// </summary>
public partial class NameComponent : Node2D
{
    /// <summary>
    /// Roxxaannnee....
    /// </summary>
    [Export] public string EntityName = "Roxanne";
}