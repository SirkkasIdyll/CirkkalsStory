using CS.SlimeFactory;
using CS.SlimeFactory.Signals;
using Godot;

namespace CS.Components.Inventory;

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

public partial class CanItemBePutInStorageSignal : CancellableSignalArgs
{
    public Node<StorableComponent> Storable;
    public Node? User;
    
    public CanItemBePutInStorageSignal(Node<StorableComponent> storable, Node? user = null)
    {
        Storable = storable;
        User = user;

    }
}

public partial class CanItemBeRemovedFromStorageSignal : CancellableSignalArgs
{
    public Node<StorableComponent> Storable;

    public CanItemBeRemovedFromStorageSignal(Node<StorableComponent> storable)
    {
        Storable = storable;
    }
}

public partial class ItemPutInStorageSignal : UserSignalArgs
{
    public Node<StorableComponent> Storable;
    public Node? User;

    public ItemPutInStorageSignal(Node<StorableComponent> storable, Node? user = null)
    {
        Storable = storable;
        User = user;
    }
}

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