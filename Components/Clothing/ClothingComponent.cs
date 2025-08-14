using CS.SlimeFactory;
using Godot;

namespace CS.Components.Clothing;

public partial class ClothingComponent : Component
{
    /// <summary>
    /// The clothing slot this clothing item occupies when worn, as well as the sprite slot it renders in
    /// </summary>
    [Export]
    public ClothingSlot ClothingSlot;
    
    /// <summary>
    /// The spritesheet which should have a default and back animation
    /// </summary>
    [Export]
    public AnimatedSprite2D? ClothingSprite;
}

/// <summary>
/// These are in order of how they get rendered, bottom-most layer to top-most layer
/// </summary>
public enum ClothingSlot
{
    Shoes,
    Bottom,
    Top,
    Belt,
    Outerwear,
    Neck,
    Inhand,
    Face,
    Hat
}