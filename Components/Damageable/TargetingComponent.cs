using Godot;

namespace CS.Components.Damageable;

/// <summary>
/// Determines what targets are valid for the ability and the AOE status of the ability during battles
/// </summary>
public partial class TargetingComponent : Node2D
{
    /// <summary>
    /// When true, will affect every available target
    /// </summary>
    [Export] public bool AreaOfEffect = false;
    
    public enum Targets
    {
        All,
        Allies,
        Enemies,
    }
    
    /// <summary>
    /// Establish what you're able to target with the skill
    /// </summary>
    [Export(PropertyHint.Enum, "All,Allies,Enemies")]
    public Targets ValidTargets = Targets.Enemies;
}