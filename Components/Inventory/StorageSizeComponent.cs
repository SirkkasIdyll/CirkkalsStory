using CS.SlimeFactory;
using Godot;

namespace CS.Components.Inventory;

public partial class StorageSizeComponent : Component
{
    /// <summary>
    /// How much total storage space to occupy
    /// </summary>
    [Export]
    public float StorageSize;
}