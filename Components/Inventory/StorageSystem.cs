using System.Diagnostics.CodeAnalysis;
using CS.Components.Clothing;
using CS.Components.Interaction;
using CS.Components.Player;
using CS.Components.UI;
using CS.Nodes.Scenes.Inventory;
using CS.Nodes.UI.CustomWindow;
using CS.SlimeFactory;
using CS.SlimeFactory.Signals;
using Godot;
using Godot.Collections;

namespace CS.Components.Inventory;

public partial class StorageSystem : NodeSystem
{
    [InjectDependency] private readonly ClothingSystem _clothingSystem = null!;
    [InjectDependency] private readonly InteractSystem _interactSystem = null!;
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
        
        _nodeManager.SignalBus.GetContextActionsSignal += OnGetContextActions;
        _nodeManager.SignalBus.InteractWithSignal += OnInteractWith;
        _nodeManager.SignalBus.ItemPutInHandSignal += OnItemPutInHand;
        _nodeManager.SignalBus.ClothingEquippedSignal += OnClothingEquipped;
        _nodeManager.SignalBus.ItemPutInStorageSignal += OnItemPutInStorage;
    }

    private void OnClothingEquipped(Node<WearsClothingComponent> node, ref ClothingEquippedSignal args)
    {
        AttachItemInvisibly(node, args.Clothing);
    }

    private void OnItemPutInHand(Node<WearsClothingComponent> node, ref ItemPutInHandSignal args)
    {
        AttachItemInvisibly(node, args.Storable);
    }

    private void OnItemPutInStorage(Node<StorageComponent> node, ref ItemPutInStorageSignal args)
    {
        AttachItemInvisibly(node, args.Storable);
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
                _clothingSystem.TryUnequipClothing((interactee, wearsClothingComponent), ClothingSlot.Inhand);
        };
    }

    /// <summary>
    /// Places item into the equipped bag if one is equipped, otherwise puts the item in hand.
    /// </summary>
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
    
    public bool TryRemoveItemFromStorage(Node<StorageComponent> node, Node<StorableComponent> item,
        [NotNullWhen(true)] out Node? removedItem)
    {
        removedItem = null;
        if (!_nodeManager.TryGetComponent<StorableComponent>(item, out var storableComponent))
            return false;
        
        node.Comp.Items.Remove(item);
        node.Comp.TotalStoredSpace -= storableComponent.Space;
        var signal = new ItemRemovedFromStorageSignal((item, storableComponent));
        _nodeManager.SignalBus.EmitItemRemovedFromStorageSignal(node, ref signal);
            
        /*if (item.Owner is Node2D itemNode2D)
        {
            itemNode2D.SetVisible(true);
            item.Owner.Reparent(node.Owner.GetParent());
        }*/

        removedItem = item;
        return true;
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

public partial class ItemPutInStorageSignal : UserSignalArgs
{
    public Node<StorableComponent> Storable;

    public ItemPutInStorageSignal(Node<StorableComponent> storable)
    {
        Storable = storable;
    }
}

public partial class ItemRemovedFromStorageSignal : UserSignalArgs
{
    public Node<StorableComponent> Storable;

    public ItemRemovedFromStorageSignal(Node<StorableComponent> storable)
    {
        Storable = storable;
    }
}