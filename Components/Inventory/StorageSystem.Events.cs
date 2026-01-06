using PC.SlimeFactory;
using PC.SlimeFactory.Signals;

namespace PC.Components.Inventory;

/// <summary>
/// ExtraSmall (Pen, paper, materials) 
/// Small (Books? Hats? Scarves? Shoes?)
/// Medium (Shirt, pants, robe, cloaks, daggers? )
/// Large (Swords, bows, armor?)
/// ExtraLarge (I really have no clue)
/// </summary>
public enum ItemSize
{
    ExtraSmall,
    Small,
    Medium,
    Large,
    ExtraLarge
}

/// <summary>
/// Check what needs to be done for an item to be put in storage,
/// without actually attempting to do the actions.
/// </summary>
public partial class CanItemBePutInStorageSignal : CancellableSignalArgs
{
    public Node<StorableComponent> Storable;
    
    public CanItemBePutInStorageSignal(Node<StorableComponent> storable)
    {
        Storable = storable;

    }
}

/// <summary>
/// Check what needs to be done for an item to removed from a storage,
/// without actually attempting to do the actions.
/// </summary>
public partial class CanItemBeRemovedFromStorageSignal : CancellableSignalArgs
{
    public Node<StorableComponent> Storable;

    public CanItemBeRemovedFromStorageSignal(Node<StorableComponent> storable)
    {
        Storable = storable;
    }
}

/// <summary>
/// Before an item can be put in storage,
/// we want to remove it from any other things it's stored in
/// </summary>
public partial class BeforeItemPutInStorageSignal : UserSignalArgs
{
    public Node<StorableComponent> Storable;

    public BeforeItemPutInStorageSignal(Node<StorableComponent> storable)
    {
        Storable = storable;
    }
}

/// <summary>
/// Successfully placed an item into storage
/// </summary>
public partial class ItemPutInStorageSignal : UserSignalArgs
{
    public Node<StorableComponent> Storable;

    public ItemPutInStorageSignal(Node<StorableComponent> storable)
    {
        Storable = storable;
    }
}

/// <summary>
/// Successfully removed an item from storage
/// </summary>
public partial class ItemRemovedFromStorageSignal : UserSignalArgs
{
    public Node<StorableComponent> Storable;

    public ItemRemovedFromStorageSignal(Node<StorableComponent> storable)
    {
        Storable = storable;
    }
}

public partial class StorageClosedSignal : UserSignalArgs;
public partial class StorageOpenedSignal : UserSignalArgs;