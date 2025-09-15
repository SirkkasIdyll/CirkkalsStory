using CS.SlimeFactory;
using Godot;

namespace CS.Components.Inventory;

public partial class StorableComponent : Component
{
    /// <summary>
    /// How much storage space the item occupies when stored
    /// </summary>
    [Export]
    public float Space;

    /// <summary>
    /// The physical size of the item
    /// Example: ExtraSmall, Small, Medium, Large, ExtraLarge
    /// </summary>
    [Export]
    public ItemSize ItemSize;
}