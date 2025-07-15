using CS.SlimeFactory;
using Godot;

namespace CS.Components.Damageable;

/// <summary>
/// Determines what targets are valid for the ability and the AOE status of the ability during battles
/// </summary>
public partial class TargetingComponent : Component
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
        Self
    }
    
    /// <summary>
    /// Establish what you're able to target with the skill
    /// </summary>
    [Export(PropertyHint.Enum, "All,Allies,Enemies,Self")]
    public Targets ValidTargets = Targets.Enemies;
}