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
    [Export] public string DisplayName = "Roxanne";

    /// <summary>
    /// Systems should aim to add onto the description rather than completely overwriting it.
    /// This allows multiple different systems to add their own descriptions.
    /// </summary>
    [Export] public string Description = "";

    /// <summary>
    /// 
    /// </summary>
    public Array<string> CombatEffects = new();
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