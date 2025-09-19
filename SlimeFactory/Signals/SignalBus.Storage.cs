using CS.Components.Inventory;

namespace CS.SlimeFactory.Signals;

public partial class SignalBus
{
    public delegate void CanItemBePutInStorageSignalHandler(Node<StorageComponent> node, ref CanItemBePutInStorageSignal args);
    public event CanItemBePutInStorageSignalHandler? CanItemBePutInStorageSignal;
    public void EmitCanItemBePutInStorageSignal(Node<StorageComponent> node, ref CanItemBePutInStorageSignal args)
    {
        CanItemBePutInStorageSignal?.Invoke(node, ref args);
    }
    
    public delegate void CanItemBeRemovedFromStorageSignalHandler(Node<StorageComponent> node, ref CanItemBeRemovedFromStorageSignal args);
    public event CanItemBeRemovedFromStorageSignalHandler? CanItemBeRemovedFromStorageSignal;
    public void EmitCanItemBeRemovedFromStorageSignal(Node<StorageComponent> node, ref CanItemBeRemovedFromStorageSignal args)
    {
        CanItemBeRemovedFromStorageSignal?.Invoke(node, ref args);
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
    
    public delegate void StorageClosedSignalHandler(Node<StorageComponent> node, ref StorageClosedSignal args);
    public event StorageClosedSignalHandler? StorageClosedSignal;
    public void EmitStorageClosedSignal(Node<StorageComponent> node, ref StorageClosedSignal args)
    {
        StorageClosedSignal?.Invoke(node, ref args);
    }
    
    public delegate void StorageOpenedSignalHandler(Node<StorageComponent> node, ref StorageOpenedSignal args);
    public event StorageOpenedSignalHandler? StorageOpenedSignal;
    public void EmitStorageOpenedSignal(Node<StorageComponent> node, ref StorageOpenedSignal args)
    {
        StorageOpenedSignal?.Invoke(node, ref args);
    }
}