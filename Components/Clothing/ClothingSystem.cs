using System;
using Godot;
using Godot.Collections;
using PC.Components.Appearance;
using PC.Components.Grid;
using PC.Components.Interaction;
using PC.Components.Inventory;
using PC.Components.Movement;
using PC.Components.Player;
using PC.Components.UI;
using PC.Nodes.Scenes.Inventory;
using PC.Nodes.UI.ContextMenu;
using PC.Nodes.UI.CustomWindow;
using PC.SlimeFactory;

namespace PC.Components.Clothing;

/// <summary>
/// When things are put in hand they aren't really equipped. They're just placed in the hand.
/// This causes a decent amount of annoying checks required because the hands can support holding storables
/// in addition to clothing items.
/// </summary>
public partial class ClothingSystem : NodeSystem
{
    [InjectDependency] private readonly AppearanceSystem _appearanceSystem = null!;
    [InjectDependency] private readonly GridSystem _gridSystem = null!;
    [InjectDependency] private readonly InteractSystem _interactSystem = null!;
    [InjectDependency] private readonly PlayerManagerSystem _playerManagerSystem = null!;
    [InjectDependency] private readonly StorageSystem _storageSystem = null!;
    [InjectDependency] private readonly UserInterfaceSystem _userInterfaceSystem = null!;

    public override void _Ready()
    {
        base._Ready();

        _nodeManager.SignalBus.BeforeItemPutInStorageSignal += OnBeforeItemPutInStorage;
        _nodeManager.SignalBus.CanItemBePutInStorageSignal += OnCanItemBePutInStorage;
        _nodeManager.SignalBus.GetContextActionsSignal += OnGetContextActions;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        
        // Open the equipment menu when the action is pressed, if possible
        if (@event.IsActionPressed("equipment"))
            TryOpenEquipmentUi();
        
        if (@event.IsActionPressed("drop_item"))
        {
            var player = _playerManagerSystem.TryGetPlayer();
            if (player == null)
                return;

            if (!_nodeManager.TryGetComponent<WearsClothingComponent>(player, out var wearsClothingComponent))
                return;

            if (Input.IsActionPressed("shift_modifier"))
                TryThrowItem((player, wearsClothingComponent));
            else
                TryDropitem((player, wearsClothingComponent));
        }
    }

    /// <summary>
    /// Before an item can be put in storage,
    /// if the item is currently being worn, unequip it.
    /// </summary>
    private void OnBeforeItemPutInStorage(Node<StorageComponent> node, ref BeforeItemPutInStorageSignal args)
    {
        if (!_nodeManager.TryGetComponent<ClothingComponent>(args.Storable, out var clothingComponent))
            return;

        if (clothingComponent.EquippedBy == null)
            return;

        if (!_nodeManager.TryGetComponent<WearsClothingComponent>(clothingComponent.EquippedBy, out var wearsClothingComponent))
            return;
        
        // Unequip the item from the mob's hands if it's in the hands
        if (args.Storable.Owner == wearsClothingComponent.ClothingSlots[ClothingSlot.Inhand])
        {
            TryUnequipClothing((clothingComponent.EquippedBy, wearsClothingComponent), ClothingSlot.Inhand);
            return;
        }

        // Unequip the item from the mob in the intended slot
        if (args.Storable.Owner == wearsClothingComponent.ClothingSlots[clothingComponent.ClothingSlot])
        {
            TryUnequipClothing((clothingComponent.EquippedBy, wearsClothingComponent), clothingComponent.ClothingSlot);
            return;
        }
    }

    /// <summary>
    /// When attempting to put something into storage that is worn,
    /// prevent the attempt if the item cannot be unequipped
    /// </summary>
    private void OnCanItemBePutInStorage(Node<StorageComponent> node, ref CanItemBePutInStorageSignal args)
    {
        var storedBy = args.Storable.Comp.StoredBy;
        
        if (storedBy == null)
            return;

        if (!_nodeManager.TryGetComponent<WearsClothingComponent>(storedBy, out var wearsClothingComponent))
            return;

        // if the storable is in hand, prevent if it can't be unequipped
        if (args.Storable.Owner == wearsClothingComponent.ClothingSlots[ClothingSlot.Inhand]
            && !CanBeUnequipped((storedBy, wearsClothingComponent), ClothingSlot.Inhand))
        {
            args.Canceled = true;
            return;
        }

        if (!_nodeManager.TryGetComponent<ClothingComponent>(args.Storable, out var clothingComponent))
            return;

        // if the storable is a worn piece of clothing, prevent if it can't be unequipped
        if (args.Storable.Owner == wearsClothingComponent.ClothingSlots[clothingComponent.ClothingSlot]
            && !CanBeUnequipped((storedBy, wearsClothingComponent), clothingComponent.ClothingSlot))
        {
            args.Canceled = true;
            return;
        }
    }

    /// <summary>
    /// Choose to either equip the clothing, or unequip it if already equipped
    /// </summary>
    private void OnContextMenuActionPressed(ContextMenu contextMenu, int index)
    {
        if (!contextMenu.IndexIdMatchesAction(index, ContextMenuAction.Equip)
            && !contextMenu.IndexIdMatchesAction(index, ContextMenuAction.Unequip))
            return;
        
        var dictionary = (Dictionary<string, Node>)contextMenu.GetItemMetadata(index);
        var node = dictionary["node"];
        var interactee = dictionary["interactee"];

        if (!_nodeManager.TryGetComponent<ClothingComponent>(node, out var clothingComponent))
            return;
        
        if (!_nodeManager.TryGetComponent<CanInteractComponent>(interactee, out var canInteractComponent))
            return;

        if (!_nodeManager.TryGetComponent<WearsClothingComponent>(interactee, out var wearsClothingComponent))
            return;

        if (!_interactSystem.InRangeUnobstructed(interactee, node, canInteractComponent.MaxInteractDistance))
            return;

        if (node == wearsClothingComponent.ClothingSlots[clothingComponent.ClothingSlot])
            TryUnequipClothing((interactee, wearsClothingComponent), clothingComponent.ClothingSlot, true);
        else
            TryEquipClothing((interactee, wearsClothingComponent), (node, clothingComponent));
    }

    /// <summary>
    /// When right-clicking, creates the context-menu button for equipping items
    /// and adds it to the context menu list
    /// </summary>
    private void OnGetContextActions(Node<InteractableComponent> node, ref GetContextActionsSignal args)
    {
        if (!_nodeManager.TryGetComponent<ClothingComponent>(node, out var clothingComponent))
            return;
        
        if (!_nodeManager.TryGetComponent<WearsClothingComponent>(args.Interactee, out var wearsClothingComponent))
            return;

        var contextMenu = args.ContextMenu;
        args.ContextMenu.IndexPressed += index => OnContextMenuActionPressed(contextMenu, (int)index);
        
        if (node.Owner == wearsClothingComponent.ClothingSlots[clothingComponent.ClothingSlot])
            contextMenu.AddItem("Unequip item", (int)ContextMenuAction.Unequip);
        else
            contextMenu.AddItem("Equip item", (int)ContextMenuAction.Equip);

        var index = contextMenu.GetItemCount() - 1;
        contextMenu.SetItemMetadata(index, new Dictionary<string, Node>()
        {
            { "node", node },
            { "interactee", args.Interactee }
        });
    }

    /// <summary>
    /// Checks if the slot exists and if it's occupied, or too large
    /// </summary>
    public bool CanItemBePutInHand(Node<WearsClothingComponent> node, Node<StorableComponent> storable)
    {
        // Check if slot exists
        if (!node.Comp.ClothingSlots.TryGetValue(ClothingSlot.Inhand, out var value))
            return false;

        // If hand is occupied, don't do anything
        if (value != null)
            return false;

        if (storable.Comp.ItemSize == ItemSize.ExtraLarge)
            return false;

        // Clothing is already equipped to a slot and apparently can't be unequipped
        if (_nodeManager.TryGetComponent<ClothingComponent>(storable, out var clothingComponent)
            && storable.Owner == node.Comp.ClothingSlots[clothingComponent.ClothingSlot]
            && !CanBeUnequipped(node, clothingComponent.ClothingSlot))
            return false;
        
        // Check other systems for reasons why this can't be put in hand
        var signal = new CanItemBePutInHandSignal(storable);
        _nodeManager.SignalBus.EmitCanItemBePutInHandSignal(node, ref signal);

        if (signal.Canceled)
            return false;

        return true;
    }

    /// <summary>
    /// Checks if the slot exists, and if it's occupied if we can unequip what's in that slot if needed
    /// </summary>
    public bool CanBeEquipped(Node<WearsClothingComponent> node, Node<ClothingComponent> clothing,
        bool unequipIfOccupied = false)
    {
        // Check if slot exists
        if (!node.Comp.ClothingSlots.TryGetValue(clothing.Comp.ClothingSlot, out var value))
            return false;
        
        // If the slot is occupied and we don't want to unequip it, return false
        if (value != null && !unequipIfOccupied)
            return false;
        
        // If we do want to unequip what's already in the slot to swap it out, check if we can
        if (value != null && unequipIfOccupied && !CanBeUnequipped(node, clothing.Comp.ClothingSlot))
            return false;

        // If we're equipping from the person's own hands, we always want that to happen
        if (clothing.Owner == node.Comp.ClothingSlots[ClothingSlot.Inhand] && !CanBeUnequipped(node, ClothingSlot.Inhand))
            return false;
        
        // Check other systems that may be preventing the clothing from being equipped
        // i.e. cursed and binded
        var signal = new IsClothingEquippableSignal(clothing);
        _nodeManager.SignalBus.EmitIsClothingEquippableSignal(node, ref signal);

        if (signal.Canceled)
            return false;
        
        return true;
    }

    /// <summary>
    /// Checks if the slot exists, if there's something in that slot, 
    /// </summary>
    public bool CanBeUnequipped(Node<WearsClothingComponent> node, ClothingSlot slot)
    {
        // Check if slot exists
        if (!node.Comp.ClothingSlots.TryGetValue(slot, out var value))
            return false;
        
        // Nothing to unequip
        if (value == null)
            return false;
        
        // Check other systems that may be preventing the clothing from being unequipped
        // i.e. cursed and binded
        var signal = new IsClothingUnequippableSignal(slot);
        _nodeManager.SignalBus.EmitIsClothingUnequippableSignal(node, ref signal);
        
        if (signal.Canceled)
            return false;
        
        return true;
    }

    public bool IsHandEmpty(Node<WearsClothingComponent> node)
    {
        return node.Comp.ClothingSlots[ClothingSlot.Inhand] == null;
    }

    public bool TryDropitem(Node<WearsClothingComponent> node)
    {
        if (!_nodeManager.TryGetComponent<CanInteractComponent>(node, out var canInteractComponent))
            return false;
        
        var inhandItem = node.Comp.ClothingSlots[ClothingSlot.Inhand];
        if (inhandItem == null)
            return false;
        
        if (!TryUnequipClothing(node, ClothingSlot.Inhand, true))
            return false;

        // Get the owner's position so we can get the vector from owner to mouse
        if (node.Owner is not Node2D node2D)
            return false;
        
        _nodeManager.TryAddComponent<MoveUntilCollideComponent>(inhandItem);
        if (!_nodeManager.TryGetComponent<MoveUntilCollideComponent>(inhandItem, out var moveUntilCollideComponent))
            return false;

        var mousePosition = GetGlobalMousePosition();
        var destinationVector = new Vector2(mousePosition.X - node2D.Position.X, mousePosition.Y - node2D.Position.Y);
        moveUntilCollideComponent.Timed = true;
        moveUntilCollideComponent.TimeRemaining = 0.05f;
        moveUntilCollideComponent.MotionVector =
            destinationVector.LimitLength(canInteractComponent.MaxInteractDistance * GridSystem.TileSize * 0.9f)
            / moveUntilCollideComponent.TimeRemaining;
       
        return true;
    }

    /// <summary>
    /// Try to equip clothing if the slot exists
    /// Set the boolean if you want to try unequipping the slot if it's occupied
    /// </summary>
    public bool TryEquipClothing(Node<WearsClothingComponent> node, Node<ClothingComponent> clothing, bool unequipIfOccupied = false)
    {
        if (!CanBeEquipped(node, clothing, unequipIfOccupied))
            return false;

        // Send a signal to other systems to have the item removed/unequipped before equipping it
        var beforeClothingEquippedSignal = new BeforeClothingEquippedSignal(clothing);
        _nodeManager.SignalBus.EmitBeforeClothingEquippedSignal(node, ref beforeClothingEquippedSignal);

        // If the item is for some reason still equipped,
        // do not proceed with equipping.
        if (clothing.Comp.EquippedBy != null)
            return false;

        // If we're already wearing something and want to swap it out, unequip the clothing
        // or if the item is in our hand, get it out of our hand
        if (node.Comp.ClothingSlots[clothing.Comp.ClothingSlot] != null && unequipIfOccupied)
            TryUnequipClothing(node, clothing.Comp.ClothingSlot);
        else if (clothing.Owner == node.Comp.ClothingSlots[ClothingSlot.Inhand])
            TryUnequipClothing(node, ClothingSlot.Inhand);
        
        // Equip the clothing to that slot
        node.Comp.ClothingSlots[clothing.Comp.ClothingSlot] = clothing;
        clothing.Comp.EquippedBy = node;
        
        // Assign the spriteFrame to the correct AnimatedSprite2D node
        // This causes the item to show up on the character's body
        var spriteSlot = node.Owner.GetNodeOrNull<AnimatedSprite2D>("CanvasGroup/" + clothing.Comp.ClothingSlot);
        if (clothing.Comp.EquippedSpriteFrames != null && spriteSlot != null)
            spriteSlot.SpriteFrames = clothing.Comp.EquippedSpriteFrames;

        // Handles clothing which by default occupy the inhand slot
        if (clothing.Comp.ClothingSlot == ClothingSlot.Inhand
            && _nodeManager.TryGetComponent<StorableComponent>(clothing, out var storableComponent))
        {
            var itemPutInHandSignal = new ItemPutInHandSignal((clothing, storableComponent));
            _nodeManager.SignalBus.EmitItemPutInHandSignal(node, ref itemPutInHandSignal);
            return true;
        }
        
        var signal = new ClothingEquippedSignal(clothing);
        _nodeManager.SignalBus.EmitClothingEquippedSignal(node, ref signal);
        return true;
    }

    /// <summary>
    /// Non-clothing items can be equipped to the hand if it is storable
    /// </summary>
    public bool TryPutItemInHand(Node<WearsClothingComponent> node, Node<StorableComponent> item)
    {
        if (!CanItemBePutInHand(node, item))
            return false;

        // Send a signal to other systems to have the item removed/unequipped before equipping it
        var beforeItemPutInHandSignal = new BeforeItemPutInHandSignal(item);
        _nodeManager.SignalBus.EmitBeforeItemPutInHandSignal(node, ref beforeItemPutInHandSignal);

        // If the item is for some reason still stored,
        // do not proceed with putting the item in hand.
        if (item.Comp.StoredBy != null)
            return false;
        
        var inHandEquippedSprite = node.Owner.GetNodeOrNull<AnimatedSprite2D>("CanvasGroup/Inhand");
        inHandEquippedSprite.SpriteFrames = null;
        
        if (_nodeManager.TryGetComponent<ClothingComponent>(item, out var clothingComponent))
        {
            // when the item being put in hand is already worn in a different slot, unequip it first
            if (item.Owner == node.Comp.ClothingSlots[clothingComponent.ClothingSlot])
                TryUnequipClothing(node, clothingComponent.ClothingSlot);
            
            // if the clothing item is intended to be inhand, show off the sprite
            // generic storables usually don't have an inhand sprite to show off
            if (clothingComponent.ClothingSlot == ClothingSlot.Inhand)
                inHandEquippedSprite.SpriteFrames = clothingComponent.EquippedSpriteFrames;
        }

        // Pickup animation
        if (node.Owner is Node2D node2D && item.Owner is Node2D itemNode2D && itemNode2D.IsVisibleInTree())
        {
            var tween = CreateTween();
            tween.TweenProperty(itemNode2D, "global_position", node2D.GlobalPosition, 0.125f);
            tween.Finished += () =>
            {
                node.Comp.ClothingSlots[ClothingSlot.Inhand] = item;
                item.Comp.StoredBy = node;
                itemNode2D.SetGlobalRotation(0); // Reset position so dropping it is normal
                
                var signal = new ItemPutInHandSignal(item);
                _nodeManager.SignalBus.EmitItemPutInHandSignal(node, ref signal);
            };
            return true;
        }
        node.Comp.ClothingSlots[ClothingSlot.Inhand] = item;
        item.Comp.StoredBy = node;
        
        var signal = new ItemPutInHandSignal(item);
        _nodeManager.SignalBus.EmitItemPutInHandSignal(node, ref signal);
        return true;
    }

    public bool TryThrowItem(Node<WearsClothingComponent> node)
    {
        if (!_nodeManager.TryGetComponent<CanInteractComponent>(node, out var canInteractComponent))
            return false;
        
        var inhandItem = node.Comp.ClothingSlots[ClothingSlot.Inhand];
        if (inhandItem == null)
            return false;

        if (inhandItem is not RigidBody2D rigidBody2D)
            return false;
        
        if (!TryUnequipClothing(node, ClothingSlot.Inhand, true))
            return false;

        var mousePosition = _gridSystem.GlobalPositionToGridPosition(GetGlobalMousePosition());
        var nodePosition = _gridSystem.GetPosition(node);

        if (nodePosition == null)
            return false;
        
        rigidBody2D.SetGlobalPosition(_gridSystem.GridPositionToGlobalPosition(nodePosition.Value));
        var distanceVector = new Vector2(mousePosition.X - nodePosition.Value.X, mousePosition.Y - nodePosition.Value.Y);
        var limitedDistanceVector = distanceVector.LimitLength(canInteractComponent.MaxInteractDistance * 3f);
        rigidBody2D.ApplyCentralImpulse(limitedDistanceVector * 160f);
        rigidBody2D.SetRotationDegrees(Random.Shared.Next(0, 360));
        
        return true;
    }
    
    public bool TryUnequipClothing(Node<WearsClothingComponent> node, ClothingSlot slot, bool dropItem = false)
    {
        if (!CanBeUnequipped(node, slot))
            return false;
        
        var clothingItem = node.Comp.ClothingSlots[slot];
        if (clothingItem == null)
            return false;
        
        // Put the item back into the world and make it visible again
        if (dropItem)
        {
            clothingItem.Reparent(node.Owner.GetParent());
            if (clothingItem is Node2D clothesNode2D)
                clothesNode2D.SetVisible(true);
        }
        
        // Removes the item from the character's clothing slots and removes the appearance of it on the character
        node.Comp.ClothingSlots[slot] = null;
        var spriteSlot = node.Owner.GetNodeOrNull<AnimatedSprite2D>("CanvasGroup/" + slot);
        if (spriteSlot != null)
            spriteSlot.SpriteFrames = null;

        if (slot == ClothingSlot.Inhand && _nodeManager.TryGetComponent<StorableComponent>(clothingItem, out var storableComponent))
        {
            storableComponent.StoredBy = null;
            var signal = new ItemRemovedFromHandSignal((clothingItem, storableComponent));
            _nodeManager.SignalBus.EmitItemRemovedFromHandSignal(node, ref signal);
        }
        else if (_nodeManager.TryGetComponent<ClothingComponent>(clothingItem, out var clothingComponent))
        {
            clothingComponent.EquippedBy = null;
            var signal = new ClothingUnequippedSignal((clothingItem, clothingComponent));
            _nodeManager.SignalBus.EmitClothingUnequippedSignal(node, ref signal);
        }

        return true;
    }

    /// <summary>
    /// Opens the equipment UI if all the requirements are met
    /// </summary>
    private void TryOpenEquipmentUi()
    {
        var player = _playerManagerSystem.TryGetPlayer();
        if (player == null)
            return;

        if (!_nodeManager.TryGetComponent<WearsClothingComponent>(player, out var wearsClothingComponent))
            return;

        if (!_nodeManager.TryGetComponent<AttachedUserInterfaceComponent>(player, out var attachedUserInterfaceComponent))
            return;
        
        var control = _userInterfaceSystem.OpenAttachedUserInterface((player, attachedUserInterfaceComponent), player, "equipment");
        if (control == null)
            return;
        
        var customWindow = (CustomWindowSystem)control;
        if (customWindow.Content == null)
            return;

        var clothingSceneSystem = (ClothingSceneSystem)customWindow.Content;
        clothingSceneSystem.ModifyScene(player);
    }
}

