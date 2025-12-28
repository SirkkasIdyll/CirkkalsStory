using System.Diagnostics.CodeAnalysis;
using CS.Components.Clothing;
using CS.Components.Interaction;
using CS.Components.Player;
using CS.Components.UI;
using CS.Nodes.Scenes.Inventory;
using CS.Nodes.UI.ContextMenu;
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

        _nodeManager.SignalBus.BeforeClothingEquippedSignal += OnBeforeClothingEquipped;
        _nodeManager.SignalBus.BeforeItemPutInHandSignal += OnBeforeItemPutInHand;
        _nodeManager.SignalBus.BeforeItemPutInStorageSignal += OnBeforeItemPutInStorage;
        _nodeManager.SignalBus.GetContextActionsSignal += OnGetContextActions;
        _nodeManager.SignalBus.InteractWithSignal += OnInteractWith;
        _nodeManager.SignalBus.IsClothingEquippableSignal += OnIsClothingEquippable;
        _nodeManager.SignalBus.ItemPutInStorageSignal += OnItemPutInStorage;
        _nodeManager.SignalBus.ItemRemovedFromStorageSignal += OnItemRemovedFromStorage;
        _nodeManager.SignalBus.StorageClosedSignal += OnStorageClosed;
        _nodeManager.SignalBus.StorageOpenedSignal += OnStorageOpened;
    }

    /// <summary>
    /// Open the inventory when the inventory button is pressed.
    /// </summary>
    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        
        if (@event.IsActionPressed("inventory"))
            TryOpenInventoryUi();
    }

    /// <summary>
    /// Before an item can be equipped,
    /// remove it from the storage it's in.
    /// </summary>
    /// <param name="node"></param>
    /// <param name="args"></param>
    private void OnBeforeClothingEquipped(Node<WearsClothingComponent> node, ref BeforeClothingEquippedSignal args)
    {
        if (!_nodeManager.TryGetComponent<StorableComponent>(args.Clothing, out var storableComponent))
            return;

        // Not stored in anything
        if (storableComponent.StoredBy == null)
            return;

        // Not in a storage, probably just worn by someone
        if (!_nodeManager.TryGetComponent<StorageComponent>(storableComponent.StoredBy, out var storageComponent))
            return;

        TryRemoveItemFromStorage((storableComponent.StoredBy, storageComponent), (args.Clothing, storableComponent), out _);
    }

    /// <summary>
    /// Before an item can be put in one's hand,
    /// remove it from the storage it's in.
    /// </summary>
    private void OnBeforeItemPutInHand(Node<WearsClothingComponent> node, ref BeforeItemPutInHandSignal args)
    {
        var storedBy = args.Storable.Comp.StoredBy;
        if (storedBy == null)
            return;

        if (!_nodeManager.TryGetComponent<StorageComponent>(storedBy, out var storageComponent))
            return;

        TryRemoveItemFromStorage((storedBy, storageComponent), args.Storable, out _);
    }

    /// <summary>
    /// Before an item can be put in storage,
    /// remove it from the storage it's in.
    /// </summary>
    private void OnBeforeItemPutInStorage(Node<StorageComponent> node, ref BeforeItemPutInStorageSignal args)
    {
        var storedBy = args.Storable.Comp.StoredBy;
        if (storedBy == null)
            return;

        if (_nodeManager.TryGetComponent<StorageComponent>(storedBy, out var storageComponent))
        {
            TryRemoveItemFromStorage((storedBy, storageComponent), args.Storable, out _);
            return;
        }

        if (_nodeManager.TryGetComponent<WearsClothingComponent>(storedBy, out var wearsClothingComponent)
            && args.Storable.Owner == wearsClothingComponent.ClothingSlots[ClothingSlot.Inhand])
        {
            _clothingSystem.TryUnequipClothing((storedBy, wearsClothingComponent), ClothingSlot.Inhand);
            return;
        }
    }

    /// <summary>
    /// Items can be dropped if in hand
    /// </summary>
    private void OnContextActionIndexPressed(ContextMenu contextMenu, int index)
    {
        if (!contextMenu.IndexIdMatchesAction(index, ContextMenuAction.Drop))
            return;
        
        var dictionary = (Dictionary<string, Node>)contextMenu.GetItemMetadata(index);
        var node = dictionary["node"];
        var interactee = dictionary["interactee"];

        if (!_nodeManager.HasComponent<StorableComponent>(node))
            return;
        
        if (!_nodeManager.TryGetComponent<CanInteractComponent>(interactee, out var canInteractComponent))
            return;

        if (!_nodeManager.TryGetComponent<WearsClothingComponent>(interactee, out var wearsClothingComponent))
            return;
        
        if (!_interactSystem.InRangeUnobstructed(interactee, node, canInteractComponent.MaxInteractDistance))
            return;
        
        _clothingSystem.TryUnequipClothing((interactee, wearsClothingComponent), ClothingSlot.Inhand, true);
    }

    /// <summary>
    /// For <see cref="StorableComponent"/> objects, when right-clicking them,
    /// give the user to drop the item in hand
    /// </summary>
    private void OnGetContextActions(Node<InteractableComponent> node, ref GetContextActionsSignal args)
    {
        if (!_nodeManager.HasComponent<StorableComponent>(node))
            return;
        
        if (!_nodeManager.TryGetComponent<WearsClothingComponent>(args.Interactee, out var wearsClothingComponent))
            return;

        var contextMenu = args.ContextMenu;
        args.ContextMenu.IndexPressed += index => OnContextActionIndexPressed(contextMenu, (int)index);
        
        if (node.Owner == wearsClothingComponent.ClothingSlots[ClothingSlot.Inhand])
            contextMenu.AddItem("Drop", (int)ContextMenuAction.Drop);

        var index = contextMenu.GetItemCount() - 1;
        contextMenu.SetItemMetadata(index, new Dictionary<string, Node>()
        {
            { "node", node },
            { "interactee", args.Interactee }
        });
    }

    /// <summary>
    /// For <see cref="StorableComponent"/> objects, when interacted with,
    /// attempts to place the object in the interactee's highest capacity equipped storage 
    /// </summary>
    private void OnInteractWith(Node<InteractableComponent> node, ref InteractWithSignal args)
    {
        if (args.Handled)
            return;
        
        if (!_nodeManager.TryGetComponent<StorableComponent>(node, out var storableComponent))
            return;

        if (!_nodeManager.TryGetComponent<WearsClothingComponent>(args.Interactee, out var wearsClothingComponent))
            return;
        
        if (_clothingSystem.TryPutItemInHand((args.Interactee, wearsClothingComponent), (node, storableComponent)))
        {
            args.Handled = true;
            return;
        }

        // if (!TryGetAvailableWornStorage((args.Interactee, wearsClothingComponent), 
        //         (node, storableComponent), out var storage))
        //     return;
        //
        // if (TryAddItemToStorage(storage.Value, (node, storableComponent)))
        //     args.Handled = true;
    }

    /// <summary>
    /// When attempting to equip clothing, if it's currently in a storage,
    /// check if it can be removed from that storage.
    /// </summary>
    private void OnIsClothingEquippable(Node<WearsClothingComponent> node, ref IsClothingEquippableSignal args)
    {
        if (!_nodeManager.TryGetComponent<StorableComponent>(args.Clothing, out var storableComponent))
            return;

        // Not stored in anything
        if (storableComponent.StoredBy == null)
            return;

        // Not in a storage, probably just worn by someone
        if (!_nodeManager.TryGetComponent<StorageComponent>(storableComponent.StoredBy, out var storageComponent))
            return;
        
        // Removable, it's fine.
        if (CanBeRemovedFromStorage((storableComponent.StoredBy, storageComponent), (args.Clothing, storableComponent)))
            return;
        
        args.Canceled = true;
    }

    /*private void OnItemPutInHand(Node<WearsClothingComponent> node, ref ItemPutInHandSignal args)
    {
        // TODO: THIS NEEDS TO BE AN EVENT IN THE CLOTHING SYSTEM THAT CAUSES THE STORAGE SYSTEM TO REMOVE IT FROM STORAGE
        // TODO: BEFORE IT CAN EVEN BE EQUIPPED
        _nodeManager.NodeQuery<StorageComponent>(out var dictionary);
        foreach (var (storage, storageComponent) in dictionary)
        {
            // Find the storage that contains the item
            if (!storageComponent.Items.Contains(args.Storable))
                continue;

            TryRemoveItemFromStorage((storage, storageComponent), args.Storable, out _);
            return;
        }
    }*/

    /// <summary>
    /// A storage increases in volume when adding items to it.
    /// This is important when storages are stored in other storages.
    /// </summary>
    private void OnItemPutInStorage(Node<StorageComponent> node, ref ItemPutInStorageSignal args)
    {
        if (!_nodeManager.TryGetComponent<StorableComponent>(node, out var storableComponent))
            return;
        
        storableComponent.Volume += args.Storable.Comp.Volume;
    }

    /// <summary>
    /// A storage decreases in volume when removing items from it.
    /// This is important when storages are stored in other storages.
    /// </summary>
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

    /// <summary>
    /// When attempting to add something to a storage,
    /// checks if the storage has the capacity to fit the item,
    /// and also if currently stored, checks if it can be removed from that storage
    /// </summary>
    public bool CanBeAddedToStorage(Node<StorageComponent> node, Node<StorableComponent> item)
    {
        // Can't store the item inside itself. No black holes!
        if (node.Owner == item.Owner)
            return false;
        
        // Item would exceed the capacity of the storage
        if (node.Comp.VolumeOccupied + item.Comp.Volume > node.Comp.Capacity)
            return false;

        // Item is too large to fit into the storage
        if (node.Comp.MaxItemSize < item.Comp.ItemSize)
            return false;

        // If currently inside a storage, check if we can remove it from that storage
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

    /// <summary>
    /// When attempting to remove something from storage,
    /// checks other systems to see if there is anything preventing it. 
    /// </summary>
    public bool CanBeRemovedFromStorage(Node<StorageComponent> node, Node<StorableComponent> item)
    {
        // Check other systems preventing item from being removed
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

        // Send a signal to other systems to have the item removed/unequipped it before storing it again
        var beforeItemAddedToStorageSignal = new BeforeItemPutInStorageSignal(item);
        _nodeManager.SignalBus.EmitBeforeItemPutInStorageSignal(node, ref beforeItemAddedToStorageSignal);
        
        // If the item is still incorrectly stored,
        // do not add the item to the storage.
        if (item.Comp.StoredBy != null)
            return false;
        
        node.Comp.Items.Add(item);
        node.Comp.VolumeOccupied += item.Comp.Volume;
        item.Comp.StoredBy = node;
        
        // Play insert SFX
        node.Comp.FluctuatingAudioStreamPlayer2DSystem?.SetStream(node.Comp.InsertSound);
        node.Comp.FluctuatingAudioStreamPlayer2DSystem?.Play();
        
        // Pickup animation but towards the storage item (if visible)
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
    /// Gets the highest capacity, available storage currently equipped.
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
    
    /// <summary>
    /// Tries to remove an item from storage
    /// </summary>
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
    
    /// <summary>
    /// When the inventory button is pressed,
    /// we see if the current mob has an inventory UI to open.
    /// </summary>
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
        inventorySceneSystem.ModifyScene(player);
    }
}