using Godot;
using PC.SlimeFactory;

namespace PC.Components.StatusEffect;

public partial class StatusEffectApplicatorComponent : Component
{
    /// <summary>
    /// The status effect to apply to a target
    /// </summary>
    [Export] public Node? StatusEffect;
}