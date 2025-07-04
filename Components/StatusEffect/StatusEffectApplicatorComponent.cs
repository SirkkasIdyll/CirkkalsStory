using CS.SlimeFactory;
using Godot;

namespace CS.Components.StatusEffect;

public partial class StatusEffectApplicatorComponent : Component
{
    /// <summary>
    /// The status effect to apply to a target
    /// </summary>
    [Export] public Node? StatusEffect;
}