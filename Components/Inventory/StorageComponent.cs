using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Components.Inventory;

public partial class StorageComponent : Component
{
    /// <summary>
    /// The volume of the container that can be used to store <see cref="StorableComponent"/>
    /// </summary>
    [Export]
    public float Capacity;
    
    /// <summary>
    /// What items are currently being stored
    /// </summary>
    [Export]
    public Array<Node> Items =  [];

    /// <summary>
    /// The largest possible item that can fit inside this storage
    /// </summary>
    [Export]
    public ItemSize MaxItemSize = ItemSize.Large;
    
    /// <summary>
    /// How much volume is currently occupied by <see cref="StorableComponent"/>
    /// </summary>
    public float VolumeOccupied;
}