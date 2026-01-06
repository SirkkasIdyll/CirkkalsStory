using PC.Components.Inventory;
using PC.SlimeFactory;
using PC.SlimeFactory.Signals;

namespace PC.Components.Clothing;

/// <summary>
/// Before clothing can be equipped,
/// it should be removed from any storage or other equipment slot it's in
/// </summary>
public partial class BeforeClothingEquippedSignal : UserSignalArgs
{
    public Node<ClothingComponent> Clothing;

    public BeforeClothingEquippedSignal(Node<ClothingComponent> clothing)
    {
        Clothing = clothing;
    }
}

public partial class BeforeItemPutInHandSignal : UserSignalArgs
{
    public Node<StorableComponent> Storable;

    public BeforeItemPutInHandSignal(Node<StorableComponent> storable)
    {
        Storable = storable;
    }
}

/// <summary>
/// Check what needs to be done to put an item into hand,
/// without actually attempting to do the actions.
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
/// Successfully equipped clothing to non-inhand slot
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
/// Successfully unequipped clothing from non-inhand slot
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
/// Check what needs to be done to equip clothing,
/// without actually attempting to do the actions.
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
/// Check what needs to be done to unequip clothing,
/// without actually attempting to do the actions.
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
/// Successfully equipped item to inhand slot.
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
/// Successfully removed item from inhand slot.
/// </summary>
public partial class ItemRemovedFromHandSignal : UserSignalArgs
{
    public Node<StorableComponent> Storable;

    public ItemRemovedFromHandSignal(Node<StorableComponent> storable)
    {
        Storable = storable;
    }
}