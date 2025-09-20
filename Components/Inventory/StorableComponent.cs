using CS.SlimeFactory;
using Godot;

namespace CS.Components.Inventory;

public partial class StorableComponent : Component
{
    /// <summary>
    /// The physical size of the item
    /// Example: ExtraSmall, Small, Medium, Large, ExtraLarge
    /// </summary>
    [Export]
    public ItemSize ItemSize;
    
    /// <summary>
    /// How much volume the item occupies when stored
    /// </summary>
    [Export]
    public float Volume;

    /// <summary>
    /// The node that is currently storing the storable
    /// Could be a person, or a storage
    /// </summary>
    public Node? StoredBy;
}