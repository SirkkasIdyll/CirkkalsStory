using CS.Components.Inventory;
using CS.SlimeFactory;
using CS.SlimeFactory.Signals;

namespace CS.Components.Clothing;

/// <summary>
/// Raised when a mob puts an item into the inhand slot
/// </summary>
public partial class CanItemBePutInHandSignal : CancellableSignalArgs
{
    public Node<StorableComponent> Storable;

    public CanItemBePutInHandSignal(Node<StorableComponent> storable)
    {
        Storable = storable;
    }
}

/// <summary>
/// Raised when a mob equips a piece of clothing to a non-inhand slot
/// </summary>
public partial class ClothingEquippedSignal : UserSignalArgs
{
    public Node<ClothingComponent> Clothing;

    public ClothingEquippedSignal(Node<ClothingComponent> clothing)
    {
        Clothing = clothing;
    }
}

/// <summary>
/// Raised when a mob unequips a piece of clothing in a non-inhand slot
/// </summary>
public partial class ClothingUnequippedSignal : UserSignalArgs
{
    public Node<ClothingComponent> Clothing;

    public ClothingUnequippedSignal(Node<ClothingComponent> clothing)
    {
        Clothing = clothing;
    }
}

/// <summary>
/// Raised when we want to check if something is preventing equipping the clothing
/// </summary>
public partial class IsClothingEquippableSignal : CancellableSignalArgs
{
    public Node<ClothingComponent> Clothing;

    public IsClothingEquippableSignal(Node<ClothingComponent> clothing)
    {
        Clothing = clothing;
    }
}

/// <summary>
/// Raised when we want to check if something is preventing unequipping the clothing
/// </summary>
public partial class IsClothingUnequippableSignal : CancellableSignalArgs
{
    public ClothingSlot ClothingSlot;

    public IsClothingUnequippableSignal(ClothingSlot clothingSlot)
    {
        ClothingSlot = clothingSlot;
    }
}

/// <summary>
/// Raised when a mob puts an item into the inhand slot
/// </summary>
public partial class ItemPutInHandSignal : UserSignalArgs
{
    public Node<StorableComponent> Storable;

    public ItemPutInHandSignal(Node<StorableComponent> storable)
    {
        Storable = storable;
    }
}

/// <summary>
/// Raised when a mob has an item removed from their inhand slot
/// </summary>
public partial class ItemRemovedFromHandSignal : UserSignalArgs
{
    public Node<StorableComponent> Storable;

    public ItemRemovedFromHandSignal(Node<StorableComponent> storable)
    {
        Storable = storable;
    }
}