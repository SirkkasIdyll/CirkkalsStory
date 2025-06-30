using Godot;
using Godot.Collections;

namespace CS.Components.Description;

/// <summary>
/// Used for things that need to be described such as skills, spells, or monsters
/// </summary>
public partial class DescriptionComponent : Node2D
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
    /// What kind of effects the ability has when used in combat
    /// </summary>
    public Array<string> CombatEffects = new();
    
    /// <summary>
    /// What the ability costs to use when in combat
    /// </summary>
    public Array<string> CombatCosts = new();
    
    public override void _Ready()
    {
        base._Ready();

        foreach (var child in GetParent().GetChildren())
        {
            if (child.HasMethod("DescribeEffect"))
                CombatEffects.Add(child.Call("DescribeEffect").AsString());
            
            if (child.HasMethod("DescribeCosts"))
                CombatEffects.Add(child.Call("DescribeCosts").AsString());
        }
    }
}