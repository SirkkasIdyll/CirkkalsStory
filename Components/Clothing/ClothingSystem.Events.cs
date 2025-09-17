using CS.Components.Inventory;
using CS.SlimeFactory;
using CS.SlimeFactory.Signals;

namespace CS.Components.Clothing;

public partial class ClothingSystem;

public partial class ClothingEquippedSignal : UserSignalArgs
{
    public Node<ClothingComponent> Clothing;

    public ClothingEquippedSignal(Node<ClothingComponent> clothing)
    {
        Clothing = clothing;
    }
}

public partial class ItemPutInHandSignal : UserSignalArgs
{
    public Node<StorableComponent> Storable;

    public ItemPutInHandSignal(Node<StorableComponent> storable)
    {
        Storable = storable;
    }
}

public partial class ClothingUnequippedSignal : UserSignalArgs
{
    public Node<ClothingComponent> Clothing;

    public ClothingUnequippedSignal(Node<ClothingComponent> clothing)
    {
        Clothing = clothing;
    }
}

public partial class ItemRemovedFromHandSignal : UserSignalArgs
{
    public Node<StorableComponent> Storable;

    public ItemRemovedFromHandSignal(Node<StorableComponent> storable)
    {
        Storable = storable;
    }
}

public partial class IsClothingEquippableSignal : CancellableSignalArgs
{
    public Node<ClothingComponent> Clothing;

    public IsClothingEquippableSignal(Node<ClothingComponent> clothing)
    {
        Clothing = clothing;
    }
}

public partial class IsClothingUnequippableSignal : CancellableSignalArgs
{
    public ClothingSlot ClothingSlot;

    public IsClothingUnequippableSignal(ClothingSlot clothingSlot)
    {
        ClothingSlot = clothingSlot;
    }
}