using System.Diagnostics.CodeAnalysis;
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
    [InjectDependency] private readonly InteractSystem _interactSystem = null!;
    [InjectDependency] private readonly PlayerManagerSystem _playerManagerSystem = null!;
    [InjectDependency] private readonly UserInterfaceSystem _userInterfaceSystem = null!;

    public override void _Ready()
    {
        base._Ready();
        
        _nodeManager.SignalBus.ClothingEquippedSignal += OnClothingEquipped;
        _nodeManager.SignalBus.GetContextActionsSignal += OnGetContextActions;
        _nodeManager.SignalBus.InteractWithSignal += OnInteractWith;
        _nodeManager.SignalBus.IsClothingEquippableSignal += OnIsClothingEquippable;
        _nodeManager.SignalBus.ItemPutInHandSignal += OnItemPutInHand;
        _nodeManager.SignalBus.ItemPutInStorageSignal += OnItemPutInStorage;
        _nodeManager.SignalBus.ItemRemovedFromStorageSignal += OnItemRemovedFromStorage;
        _nodeManager.SignalBus.StorageClosedSignal += OnStorageClosed;
        _nodeManager.SignalBus.StorageOpenedSignal += OnStorageOpened;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        
        if (@event.IsActionPressed("inventory"))
            TryOpenInventoryUi();
    }

    private void OnClothingEquipped(Node<WearsClothingComponent> node, ref ClothingEquippedSignal args)
    {
        AttachItemInvisibly(node, args.Clothing);
        
        _nodeManager.NodeQuery<StorageComponent>(out var dictionary);
        foreach (var (storage, storageComponent) in dictionary)
        {
            // Find the storage that contains the item
            if (!storageComponent.Items.Contains(args.Clothing))
                continue;
            
            if (!_nodeManager.TryGetComponent<StorableComponent>(args.Clothing, out var storableComponent))
                return;

            TryRemoveItemFromStorage((storage, storageComponent), (args.Clothing, storableComponent), out _);
            return;
        }
    }

    private void OnGetContextActions(Node<InteractableComponent> node, ref GetContextActionsSignal args)
    {
        var button = new Button();
        args.Actions.Add(button);
        button.SetText("Pick up");
        
        // Disable the equip item option for users that are incapable of interacting
        if (!_nodeManager.TryGetComponent<CanInteractComponent>(args.Interactee, out var canInteractComponent))
        {
            button.Disabled = true;
            return;
        }
        
        // Disable the equip item option for users that are incapable of wearing clothing
        if (!_nodeManager.TryGetComponent<WearsClothingComponent>(args.Interactee, out var wearsClothingComponent))
        {
            button.Disabled = true;
            return;
        }

        if (!_nodeManager.TryGetComponent<StorableComponent>(node, out var storableComponent))
            return;
        
        if (_nodeManager.TryGetComponent<ClothingComponent>(node, out var clothingComponent)
            && node.Owner == wearsClothingComponent.ClothingSlots[clothingComponent.ClothingSlot])
            button.SetText("Put item in hand");
        
        if (node.Owner == wearsClothingComponent.ClothingSlots[ClothingSlot.Inhand])
            button.SetText("Drop item");
        
        // Set the button up to equip the item if the user is in-range when pressed
        var interactee = args.Interactee;
        button.Pressed += () =>
        {
            if (!_interactSystem.InRangeUnobstructed(interactee, node.Owner, canInteractComponent.MaxInteractDistance))
                return;

            if (node.Owner != wearsClothingComponent.ClothingSlots[ClothingSlot.Inhand])
                _clothingSystem.TryPutItemInHand((interactee, wearsClothingComponent), (node, storableComponent));
            else
                _clothingSystem.TryUnequipClothing((interactee, wearsClothingComponent), ClothingSlot.Inhand, true);
        };
    }

    /// <summary>
    /// Places storable items into the highest capacity equipped storage when interacted with
    /// </summary>
    private void OnInteractWith(Node<InteractableComponent> node, ref InteractWithSignal args)
    {
        if (args.Handled)
            return;
        
        if (!_nodeManager.TryGetComponent<StorableComponent>(node, out var storableComponent))
            return;

        if (!_nodeManager.TryGetComponent<WearsClothingComponent>(args.Interactee, out var wearsClothingComponent))
            return;

        if (!TryGetAvailableWornStorage((args.Interactee, wearsClothingComponent), 
                (node, storableComponent), out var storage))
            return;
        
        if (TryAddItemToStorage(storage.Value, (node, storableComponent)))
            args.Handled = true;
    }

    private void OnIsClothingEquippable(Node<WearsClothingComponent> node, ref IsClothingEquippableSignal args)
    {
        _nodeManager.NodeQuery<StorageComponent>(out var dictionary);
        foreach (var (storage, storageComponent) in dictionary)
        {
            // Find the storage that contains the item
            if (!storageComponent.Items.Contains(args.Clothing))
                continue;
            
            if (!_nodeManager.TryGetComponent<StorableComponent>(args.Clothing, out var storableComponent))
                continue;

            // If the item can't be removed from the storage, cancel the attempt to equip the clothing item
            if (!CanBeRemovedFromStorage((storage, storageComponent), (args.Clothing, storableComponent)))
                args.Canceled = true;
            
            return;
        }
    }

    private void OnItemPutInHand(Node<WearsClothingComponent> node, ref ItemPutInHandSignal args)
    {
        AttachItemInvisibly(node, args.Storable);
        
        _nodeManager.NodeQuery<StorageComponent>(out var dictionary);
        foreach (var (storage, storageComponent) in dictionary)
        {
            // Find the storage that contains the item
            if (!storageComponent.Items.Contains(args.Storable))
                continue;

            TryRemoveItemFromStorage((storage, storageComponent), args.Storable, out _);
            return;
        }
    }

    private void OnItemPutInStorage(Node<StorageComponent> node, ref ItemPutInStorageSignal args)
    {
        // TODO: ITEM SHOULD ALREADY BE REMOVED BEFORE PUTTING ITEM IN STORAGE
        // Remove item from previously belonged to storage, if we're transferring between storages
        if (args.Storable.Comp.StoredBy != null && _nodeManager.TryGetComponent<StorageComponent>(args.Storable.Comp.StoredBy, out var storageComponent))
            TryRemoveItemFromStorage((args.Storable.Comp.StoredBy, storageComponent), args.Storable, out _);
        
        // TODO: If StoredBy isn't null at this point, throw an error
        AttachItemInvisibly(node, args.Storable);

        if (!_nodeManager.TryGetComponent<StorableComponent>(node, out var storableComponent))
            return;
        
        storableComponent.Volume += args.Storable.Comp.Volume;
    }

    private void OnItemRemovedFromStorage(Node<StorageComponent> node, ref ItemRemovedFromStorageSignal args)
    {
        if (!_nodeManager.TryGetComponent<StorableComponent>(node, out var storableComponent))
            return;
        
        storableComponent.Volume -= args.Storable.Comp.Volume;
    }

    /// <summary>
    /// Plays SFX when storage is closed
    /// </summary>
    private void OnStorageClosed(Node<StorageComponent> node, ref StorageClosedSignal args)
    {
        node.Comp.FluctuatingAudioStreamPlayer2DSystem?.SetStream(node.Comp.CloseSound);
        node.Comp.FluctuatingAudioStreamPlayer2DSystem?.Play();
    }

    /// <summary>
    /// Plays SFX when storage is opened
    /// </summary>
    private void OnStorageOpened(Node<StorageComponent> node, ref StorageOpenedSignal args)
    {
        node.Comp.FluctuatingAudioStreamPlayer2DSystem?.SetStream(node.Comp.OpenSound);
        node.Comp.FluctuatingAudioStreamPlayer2DSystem?.Play();
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
    /// Checks if the storage has the capacity to fit the item,
    /// and checks if the item can be removed from the node it belongs to
    /// </summary>
    /// <param name="node"></param>
    /// <param name="item"></param>
    /// <param name="itemBelongsTo">The item currently is equipped or stored by this node</param>
    public bool CanBeAddedToStorage(Node<StorageComponent> node, Node<StorableComponent> item)
    {
        // Can't store the item inside itself. No black holes!
        if (node.Owner == item.Owner)
            return false;
        
        // Item would exceed the capacity of the storage
        // TODO: Add a written reason why it failed that pops up
        if (node.Comp.VolumeOccupied + item.Comp.Volume > node.Comp.Capacity)
            return false;

        // Item is too large to fit into the storage
        // TODO: Add a written reason why it failed that pops up
        if (node.Comp.MaxItemSize < item.Comp.ItemSize)
            return false;

        // If currently stored elsewhere, check if we can remove it from that storage
        if (item.Comp.StoredBy != null
            && _nodeManager.TryGetComponent<StorageComponent>(item.Comp.StoredBy, out var storageComponent)
            && !CanBeRemovedFromStorage((item.Comp.StoredBy, storageComponent), item))
            return false;

        // Check other systems if they prevent it
        var signal = new CanItemBePutInStorageSignal(item);
        _nodeManager.SignalBus.EmitCanItemBePutInStorageSignal(node, ref signal);

        if (signal.Canceled)
            return false;
        
        return true;
    }

    public bool CanBeRemovedFromStorage(Node<StorageComponent> node, Node<StorableComponent> item)
    {
        // Check any other systems preventing item from being removed
        var signal = new CanItemBeRemovedFromStorageSignal(item);
        _nodeManager.SignalBus.EmitCanItemBeRemovedFromStorageSignal(node, ref signal);

        if (signal.Canceled)
            return false;
        
        return true;
    }

    /// <summary>
    /// Returns all the items currently inside a storage
    /// </summary>
    public Array<Node> GetStorageItems(Node<StorageComponent> node)
    {
        return node.Comp.Items;
    }

    /// <summary>
    /// How filled the storage is compared to the maximum, as a percentage
    /// </summary>
    public float GetStoragePercentage(Node<StorageComponent> node)
    {
        return node.Comp.VolumeOccupied / node.Comp.Capacity * 100;
    }

    /// <summary>
    /// Attempts to add an item to a storage, but fails if there is not enough capacity or if the item is too large
    /// If true, handle it and remove the item from wherever it is
    /// </summary>
    public bool TryAddItemToStorage(Node<StorageComponent> node, Node<StorableComponent> item)
    {
        if (!CanBeAddedToStorage(node, item))
            return false;
        
        // If currently stored elsewhere, check if we can remove it from that storage
        if (item.Comp.StoredBy != null)
        {
            if (_nodeManager.TryGetComponent<StorageComponent>(item.Comp.StoredBy, out var storageComponent))
                TryRemoveItemFromStorage((item.Comp.StoredBy, storageComponent), item, out _);

            if (_nodeManager.TryGetComponent<WearsClothingComponent>(item.Comp.StoredBy,
                    out var wearsClothingComponent))
            {
                if (item.Owner == wearsClothingComponent.ClothingSlots[ClothingSlot.Inhand])
                    _clothingSystem.TryUnequipClothing((item.Comp.StoredBy, wearsClothingComponent),
                        ClothingSlot.Inhand);
            }
            
        }
        
        node.Comp.Items.Add(item);
        node.Comp.VolumeOccupied += item.Comp.Volume;
        item.Comp.StoredBy = node;
        
        // Play insert SFX
        node.Comp.FluctuatingAudioStreamPlayer2DSystem?.SetStream(node.Comp.InsertSound);
        node.Comp.FluctuatingAudioStreamPlayer2DSystem?.Play();
        
        // Pickup animation
        if (node.Owner is Node2D node2D && item.Owner is Node2D itemNode2D && itemNode2D.IsVisibleInTree())
        {
            var tween = CreateTween();
            tween.TweenProperty(itemNode2D, "global_position", node2D.GlobalPosition, 0.125f);
            tween.Finished += () =>
            {
                var signal = new ItemPutInStorageSignal(item);
                _nodeManager.SignalBus.EmitItemPutInStorageSignal(node, ref signal);
            };
            return true;
        }

        var signal = new ItemPutInStorageSignal(item);
        _nodeManager.SignalBus.EmitItemPutInStorageSignal(node, ref signal);
        return true;
    }

    /// <summary>
    /// Tries to retrieve the storage node that is worn,
    /// that has the most capacity,
    /// and is capable of storing the given item.
    /// </summary>
    public bool TryGetAvailableWornStorage(Node<WearsClothingComponent> node, Node<StorableComponent> item,
        [NotNullWhen(true)] out Node<StorageComponent>? storage)
    {
        storage = null;
        foreach (var (_, clothingItem) in node.Comp.ClothingSlots)
        {
            if (clothingItem == null)
                continue;
            
            if (!_nodeManager.TryGetComponent<StorageComponent>(clothingItem, out var storageComponent))
                continue;

            // If we haven't yet picked a storage, pick the first available one
            if (storage == null && CanBeAddedToStorage((clothingItem, storageComponent), item))
            {
                storage = (clothingItem, storageComponent);
                continue;
            }
            
            // When we do have a storage picked,
            if (storage == null)
                continue;
            
            // If the capacity of the currently viewed option is greater
            if (storage.Value.Comp.Capacity > storageComponent.Capacity)
                continue;

            // And we're capable of adding the item to the currently viewed option
            if (!CanBeAddedToStorage((clothingItem, storageComponent), item))
                continue;
            
            // Pick it as the designated storage instead
            storage = (clothingItem, storageComponent);
        }
        
        return storage != null;
    }

    public bool TryRemoveItemFromStorage(Node<StorageComponent> node, Node<StorableComponent> item,
        [NotNullWhen(true)] out Node? removedItem)
    {
        removedItem = null;
        if (!CanBeRemovedFromStorage(node, item))
            return false;
        
        node.Comp.Items.Remove(item);
        node.Comp.VolumeOccupied -= item.Comp.Volume;
        item.Comp.StoredBy = null;
        
        // Play removal SFX
        node.Comp.FluctuatingAudioStreamPlayer2DSystem?.SetStream(node.Comp.RemoveSound);
        node.Comp.FluctuatingAudioStreamPlayer2DSystem?.Play();
        
        var signal = new ItemRemovedFromStorageSignal(item);
        _nodeManager.SignalBus.EmitItemRemovedFromStorageSignal(node, ref signal);

        removedItem = item;
        return true;
    }

    // Opens the inventory UI if all the requirements are met
    private void TryOpenInventoryUi()
    {
        var player = _playerManagerSystem.TryGetPlayer();
        if (player == null)
            return;

        // Only mobs that can wear clothes have an inventory, at the moment
        if (!_nodeManager.TryGetComponent<WearsClothingComponent>(player, out var wearsClothingComponent))
            return;
        
        if (!_nodeManager.TryGetComponent<AttachedUserInterfaceComponent>(player, out var attachedUserInterfaceComponent))
            return;

        var control = _userInterfaceSystem.OpenAttachedUserInterface((player, attachedUserInterfaceComponent), player, "inventory");
        if (control == null)
            return;

        var customWindow = (CustomWindowSystem)control;
        if (customWindow.Content == null)
            return;
        
        var inventorySceneSystem = (InventorySceneSystem)customWindow.Content;
        inventorySceneSystem.SetDetails((player, wearsClothingComponent));
    }
}