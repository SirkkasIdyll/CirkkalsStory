using CS.Components.Inventory;

namespace CS.SlimeFactory.Signals;

public partial class SignalBus
{
    public delegate void CanBePutInStorageSignalHandler(Node<StorageComponent> node, ref CanBePutInStorageSignal args);
    public event CanBePutInStorageSignalHandler? CanBePutInStorageSignal;
    public void EmitCanBePutInStorageSignal(Node<StorageComponent> node, ref CanBePutInStorageSignal args)
    {
        CanBePutInStorageSignal?.Invoke(node, ref args);
    }
    
    public delegate void CanBeRemovedFromStorageSignalHandler(Node<StorageComponent> node, ref CanBeRemovedFromStorageSignal args);
    public event CanBeRemovedFromStorageSignalHandler? CanBeRemovedFromStorageSignal;
    public void EmitCanBeRemovedFromStorageSignal(Node<StorageComponent> node, ref CanBeRemovedFromStorageSignal args)
    {
        CanBeRemovedFromStorageSignal?.Invoke(node, ref args);
    }
    
    public delegate void ItemPutInStorageSignalHandler(Node<StorageComponent> node, ref ItemPutInStorageSignal args);
    public event ItemPutInStorageSignalHandler? ItemPutInStorageSignal;
    public void EmitItemPutInStorageSignal(Node<StorageComponent> node, ref ItemPutInStorageSignal args)
    {
        ItemPutInStorageSignal?.Invoke(node, ref args);
    }
    
    public delegate void ItemRemovedFromStorageSignalHandler(Node<StorageComponent> node, ref ItemRemovedFromStorageSignal args);
    public event ItemRemovedFromStorageSignalHandler? ItemRemovedFromStorageSignal;
    public void EmitItemRemovedFromStorageSignal(Node<StorageComponent> node, ref ItemRemovedFromStorageSignal args)
    {
        ItemRemovedFromStorageSignal?.Invoke(node, ref args);
    }
}