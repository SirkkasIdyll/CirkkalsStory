using CS.SlimeFactory;
using CS.SlimeFactory.Signals;

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

public partial class CanBePutInStorageSignal : CancellableSignalArgs
{
    public Node<StorableComponent> Storable;

    public CanBePutInStorageSignal(Node<StorableComponent> storable)
    {
        Storable = storable;
    }
}

public partial class CanBeRemovedFromStorageSignal : CancellableSignalArgs
{
    public Node<StorableComponent> Storable;

    public CanBeRemovedFromStorageSignal(Node<StorableComponent> storable)
    {
        Storable = storable;
    }
}

public partial class ItemPutInStorageSignal : UserSignalArgs
{
    public Node<StorableComponent> Storable;

    public ItemPutInStorageSignal(Node<StorableComponent> storable)
    {
        Storable = storable;
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