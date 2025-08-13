using CS.SlimeFactory;
using CS.SlimeFactory.Signals;
using Godot;
using Godot.Collections;

namespace CS.Components.Description;

/// <summary>
/// Used for things that need to be described such as skills, spells, or monsters
/// </summary>
public partial class DescriptionComponent : Component
{
    /// <summary>
    /// Roxxaannnee....
    /// </summary>
    [Export(PropertyHint.PlaceholderText, "Roxanne")] public string DisplayName = "";

    /// <summary>
    /// Systems should aim to add onto the description rather than completely overwriting it.
    /// This allows multiple different systems to add their own descriptions.
    /// </summary>
    [Export(PropertyHint.PlaceholderText, "You don't have to put on the red light")] public string Description = "";

    /// <summary>
    /// Systems can add onto the detailed summary to give an indicator as to their current status,
    /// like PowerSystems reporting their current power level, or better informing what someone is currently wearing/wielding.
    /// </summary>
    public Array<string> DetailedSummary = [];

    /// <summary>
    /// What kind of effects the ability has when used in combat
    /// </summary>
    public Array<string> Effects = new();
    
    /// <summary>
    /// What the ability costs to use when in combat
    /// </summary>
    public Array<string> Costs = new();
}

public partial class GetActionEffectsDescriptionSignal : UserSignalArgs
{
    
}

public partial class GetActionCostsDescriptionSignal : UserSignalArgs
{
    
}

public partial class UpdateDisplayNameSignal : UserSignalArgs
{
    
}