using Godot;

namespace CS.Scripts.Components.Damageable;

/// <summary>
/// Determines what targets are valid for the ability and the AOE status of the ability during battles
/// </summary>
public partial class TargetComponent : Node2D
{
    /// <summary>
    /// When true, will target every available target based on cursor choices
    /// </summary>
    [Export] public bool AreaOfEffect = false;
    
    public enum Targets
    {
        All,
        Allies,
        Enemies,
    }

    [Export] public Targets TargetOptions;
}