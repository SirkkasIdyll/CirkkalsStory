using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Components.Inventory;

public partial class StorageSystem : NodeSystem
{
    public override void _Ready()
    {
        base._Ready();

        // _nodeManager.SignalBus.InteractWithSignal += OnInteractWith;
    }

    /*private void OnInteractWith(Node<InteractableComponent> node, ref InteractWithSignal args)
    {
        if (!_nodeManager.TryGetComponent<StorageComponent>(node, out var storageComponent))
            return;

        OpenStorageWindow((node, storageComponent));
    }

    private void OpenStorageWindow(Node<StorageComponent> node)
    {
        var customWindow = ResourceLoader.Load<PackedScene>("res://Nodes/UI/CustomWindow/CustomWindow.tscn")
            .Instantiate<CustomWindowSystem>();
        customWindow.Title.Text = "Inventory";
        GetParent().GetNode<CanvasLayer>("CanvasLayer").AddChild(customWindow);
    }*/

    /// <summary>
    /// Returns all the items currently inside a storage
    /// </summary>
    public Array<Node> GetStorageItems(Node<StorageComponent> node)
    {
        return node.Comp.Items;
    }

    /// <summary>
    /// Attempts to add an item to a storage, but fails if there is not enough space or if the item is too large
    /// If true, handle it and remove the item from wherever it is
    /// </summary>
    public bool TryAddItemToStorage(Node<StorageComponent> node, Node<StorableComponent> item)
    {
        // TODO: Add a written reason why it failed that pops up
        if (!_nodeManager.TryGetComponent<StorableComponent>(item, out var storableComponent))
            return false;

        // TODO: Add a written reason why it failed that pops up
        if (node.Comp.TotalStoredSpace + storableComponent.Space > node.Comp.MaxSpace)
            return false;

        // TODO: Add a written reason why it failed that pops up
        if (node.Comp.MaxItemSize < storableComponent.ItemSize)
            return false;
        
        node.Comp.Items.Add(item);
        node.Comp.TotalStoredSpace += storableComponent.Space;
        if (node.Owner is Node2D storageNode2D && item.Owner is Node2D itemNode2D)
        {
            itemNode2D.SetVisible(false);
            item.Owner.Reparent(node.Owner, false);
            itemNode2D.GlobalPosition = storageNode2D.GlobalPosition;
        }
        return true;
    }
    
    public Node RemoveItemFromStorage(Node<StorageComponent> node, Node item)
    {
        node.Comp.Items.Remove(item);
        if (_nodeManager.TryGetComponent<StorableComponent>(item, out var storableComponent))
            node.Comp.TotalStoredSpace -= storableComponent.Space;
        if (item.Owner is Node2D itemNode2D)
        {
            itemNode2D.SetVisible(true);
            item.Owner.Reparent(node.Owner.GetParent());
        }
        return item;
    }
}

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
