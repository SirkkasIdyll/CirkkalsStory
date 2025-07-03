using CS.SlimeFactory;
using CS.SlimeFactory.Signals;
using Godot;

namespace CS.Components.Damage;

/// <summary>
/// Dictates the amount of damage a node is capable of doing
/// </summary>
public partial class DamageComponent : Component
{
    /// <summary>
    /// The amount of damage dealt
    /// </summary>
    [Export] public int Damage;
}

/// <summary>
/// Raised when attempting to damage a target
/// </summary>
public partial class DamageAttemptSignal : HandledSignalArgs
{
    public DamageAttemptSignal(Node? attacker = null, Node? defender = null)
    {
        Attacker = attacker;
        Defender = defender;
    }
    
    public Node? Attacker;
    public Node? Defender;
}