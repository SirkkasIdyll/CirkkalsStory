using CS.SlimeFactory;
using CS.SlimeFactory.Signals;
using Godot;
using Godot.Collections;

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
    [Export] public bool IsPermanent = false;

    /// <summary>
    /// When true, turns stack on top of the existing duration
    /// </summary>
    [Export] public bool StacksDuration = false;
    
    /// <summary>
    /// The base amount of turns set each time the skill is used
    /// </summary>
    [Export] public int TurnsPerApplication;
    
    /// <summary>
    /// How many turns until the status effect expires
    /// </summary>
    public int StatusDuration;
}

public partial class ProcStatusEffectSignal : UserSignalArgs
{
    public Node Target;
    public Array<string> Summaries = [];

    public ProcStatusEffectSignal(Node target)
    {
        this.Target = target;
    }
}