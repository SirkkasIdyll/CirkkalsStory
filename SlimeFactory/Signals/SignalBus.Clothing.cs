using CS.Components.Clothing;
using CS.Components.Inventory;

namespace CS.SlimeFactory.Signals;

public partial class SignalBus
{
    public delegate void CanItemBePutInHandSignalHandler(Node<WearsClothingComponent> node,
        ref CanItemBePutInHandSignal args);
    public event CanItemBePutInHandSignalHandler? CanItemBePutInHandSignal;
    public void EmitCanItemBePutInHandSignal(Node<WearsClothingComponent> node, ref CanItemBePutInHandSignal args)
    {
        CanItemBePutInHandSignal?.Invoke(node, ref args);
    }
    
    public delegate void ClothingEquippedSignalHandler(Node<WearsClothingComponent> node,
        ref ClothingEquippedSignal args);
    public event ClothingEquippedSignalHandler? ClothingEquippedSignal;
    public void EmitClothingEquippedSignal(Node<WearsClothingComponent> node, ref ClothingEquippedSignal args)
    {
        ClothingEquippedSignal?.Invoke(node, ref args);
    }
    
    public delegate void ClothingUnequippedSignalHandler(Node<WearsClothingComponent> node,
        ref ClothingUnequippedSignal args);
    public event ClothingUnequippedSignalHandler? ClothingUnequippedSignal;
    public void EmitClothingUnequippedSignal(Node<WearsClothingComponent> node, ref ClothingUnequippedSignal args)
    {
        ClothingUnequippedSignal?.Invoke(node, ref args);
    }
    
    public delegate void ItemPutInHandSignalHandler(Node<WearsClothingComponent> node,
        ref ItemPutInHandSignal args);
    public event ItemPutInHandSignalHandler? ItemPutInHandSignal;
    public void EmitItemPutInHandSignal(Node<WearsClothingComponent> node, ref ItemPutInHandSignal args)
    {
        ItemPutInHandSignal?.Invoke(node, ref args);
    }
    
    public delegate void ItemRemovedFromHandSignalHandler(Node<WearsClothingComponent> node,
        ref ItemRemovedFromHandSignal args);
    public event ItemRemovedFromHandSignalHandler? ItemRemovedFromHandSignal;
    public void EmitItemRemovedFromHandSignal(Node<WearsClothingComponent> node, ref ItemRemovedFromHandSignal args)
    {
        ItemRemovedFromHandSignal?.Invoke(node, ref args);
    }

    public delegate void IsClothingEquippableSignalHandler(Node<WearsClothingComponent> node,
        ref IsClothingEquippableSignal args);
    public event IsClothingEquippableSignalHandler? IsClothingEquippableSignal;
    public void EmitIsClothingEquippableSignal(Node<WearsClothingComponent> node, ref IsClothingEquippableSignal args)
    {
        IsClothingEquippableSignal?.Invoke(node, ref args);
    }

    public delegate void IsClothingUnequippableSignalHandler(Node<WearsClothingComponent> node,
        ref IsClothingUnequippableSignal args);
    public event IsClothingUnequippableSignalHandler? IsClothingUnequippableSignal;
    public void EmitIsClothingUnequippableSignal(Node<WearsClothingComponent> node,
        ref IsClothingUnequippableSignal args)
    {
        IsClothingUnequippableSignal?.Invoke(node, ref args);
    }
}