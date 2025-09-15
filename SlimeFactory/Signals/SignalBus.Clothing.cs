using CS.Components.Clothing;
using CS.Components.Inventory;

namespace CS.SlimeFactory.Signals;

public partial class SignalBus
{
    public delegate void ItemPutInHandSignalHandler(Node<WearsClothingComponent> node,
        ref ItemPutInHandSignal args);
    public event ItemPutInHandSignalHandler? ItemPutInHandSignal;
    public void EmitItemPutInHandSignal(Node<WearsClothingComponent> node, ref ItemPutInHandSignal args)
    {
        ItemPutInHandSignal?.Invoke(node, ref args);
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
}