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
    /// Each item has a <see cref="StorageSizeComponent"/> with a storage size
    /// This storage is capable of storing up to the total storage size of all items inside it
    /// </summary>
    [Export]
    public float StorageLimit;
}