using CS.SlimeFactory;
using Godot;

namespace CS.Components.StatusEffect;

/// <summary>
/// A status effect is a condition that affects a creature that can either be permanent for passive buffs/debuffs
/// or timed in the case of temporary ailments
/// </summary>
public partial class StatusEffectComponent : Component
{
    /// <summary>
    /// If the status effect is permanent, it does not expire after a certain number of terns.
    /// </summary>
    public bool IsPermanent = false;

    /// <summary>
    /// How many turns until the status effect expires
    /// </summary>
    private int _duration;

    [Signal]
    public delegate void StatusEffectExpiredEventHandler(Node status);
    
    public void AlterDuration(int amount)
    {
        if (IsPermanent)
            return;
        
        _duration += amount;

        if (_duration <= 0)
            EmitSignal(SignalName.StatusEffectExpired);
    }
}