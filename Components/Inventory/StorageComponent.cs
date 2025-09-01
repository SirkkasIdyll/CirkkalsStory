using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Components.Inventory;

public partial class StorageComponent : Component
{
    /// <summary>
    /// What items are currently being stored
    /// </summary>
    [Export]
    public Array<Node> Items =  [];

    /// <summary>
    /// The maximum amount of space this storage can allot
    /// </summary>
    [Export]
    public float MaxSpace;
    
    /// <summary>
    /// How much space is currently occupied by items
    /// </summary>
    public float TotalStoredSpace;

    /// <summary>
    /// The largest possible item that can fit inside this storage
    /// </summary>
    [Export]
    public ItemSize MaxItemSize = ItemSize.Large;
}