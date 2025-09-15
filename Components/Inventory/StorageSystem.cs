using CS.Components.Clothing;
using CS.Components.Interaction;
using CS.Components.Player;
using CS.Components.UI;
using CS.Nodes.Scenes.Inventory;
using CS.Nodes.UI.CustomWindow;
using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Components.Inventory;

public partial class StorageSystem : NodeSystem
{
    [InjectDependency] private readonly ClothingSystem _clothingSystem = null!;
    [InjectDependency] private readonly PlayerManagerSystem _playerManagerSystem = null!;
    [InjectDependency] private readonly UserInterfaceSystem _userInterfaceSystem = null!;

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        
        if (@event.IsActionPressed("inventory"))
            TryOpenInventoryUi();
    }

    public override void _Ready()
    {
        base._Ready();
        
        _nodeManager.SignalBus.InteractWithSignal += OnInteractWith;
    }

    private void OnInteractWith(Node<InteractableComponent> node, ref InteractWithSignal args)
    {
        if (args.Handled)
            return;
        
        if (!_nodeManager.TryGetComponent<StorableComponent>(node, out var storableComponent))
            return;

        if (!_nodeManager.TryGetComponent<WearsClothingComponent>(args.Interactee, out var wearsClothingComponent))
            return;

        var bag = wearsClothingComponent.ClothingSlots[ClothingSlot.Bag];
        if (bag != null
            && _nodeManager.TryGetComponent<StorageComponent>(bag, out var storageComponent)
            && TryAddItemToStorage((bag, storageComponent), (node, storableComponent)))
        {
            args.Handled = true;
            return;
        }

        if (_clothingSystem.TryPutItemInHand((args.Interactee, wearsClothingComponent), (node, storableComponent)))
            args.Handled = true;
    }

    /// <summary>
    /// How filled the storage is compared to the maximum, as a percentage
    /// </summary>
    public float GetStoragePercentage(Node<StorageComponent> node)
    {
        return node.Comp.TotalStoredSpace / node.Comp.MaxSpace * 100;
    }

    /// <summary>
    /// Returns all the items currently inside a storage
    /// </summary>
    public Array<Node> GetStorageItems(Node<StorageComponent> node)
    {
        return node.Comp.Items;
    }

    public void AttachItemInvisibly(Node main, Node nodeToAttach)
    {
        if (main is not Node2D mainNode2D || nodeToAttach is not Node2D node2DToAttach)
            return;
        
        node2DToAttach.SetVisible(false);
        nodeToAttach.Reparent(main, false);
        node2DToAttach.GlobalPosition = mainNode2D.GlobalPosition;
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
        AttachItemInvisibly(node, item);
        return true;
    }
    
    // Opens the inventory UI if all the requirements are met
    private void TryOpenInventoryUi()
    {
        var player = _playerManagerSystem.TryGetPlayer();
        if (player == null)
            return;

        if (!_nodeManager.TryGetComponent<WearsClothingComponent>(player, out var wearsClothingComponent))
            return;

        wearsClothingComponent.ClothingSlots.TryGetValue(ClothingSlot.Bag, out var bagItem);
        if (bagItem == null)
            return;

        if (!_nodeManager.TryGetComponent<StorageComponent>(bagItem, out var storageComponent))
            return;
            
        if (!_nodeManager.TryGetComponent<AttachedUserInterfaceComponent>(player, out var attachedUserInterfaceComponent))
            return;

        var control = _userInterfaceSystem.OpenAttachedUserInterface((player, attachedUserInterfaceComponent), player, "inventory");
        if (control == null)
            return;

        var customWindow = (CustomWindowSystem)control;
        if (customWindow.Content == null)
            return;
        
        var storageSceneSystem = (StorageSceneSystem)customWindow.Content;
        storageSceneSystem.SetDetails((bagItem, storageComponent));
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
