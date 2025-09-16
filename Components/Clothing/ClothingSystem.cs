using CS.Components.Appearance;
using CS.Components.Interaction;
using CS.Components.Inventory;
using CS.Components.Player;
using CS.Components.UI;
using CS.Nodes.Scenes.Inventory;
using CS.Nodes.UI.CustomWindow;
using CS.SlimeFactory;
using CS.SlimeFactory.Signals;
using Godot;

namespace CS.Components.Clothing;

public partial class ClothingSystem : NodeSystem
{
    [InjectDependency] private readonly AppearanceSystem _appearanceSystem = null!;
    [InjectDependency] private readonly InteractSystem _interactSystem = null!;
    [InjectDependency] private readonly PlayerManagerSystem _playerManagerSystem = null!;
    [InjectDependency] private readonly StorageSystem _storageSystem = null!;
    [InjectDependency] private readonly UserInterfaceSystem _userInterfaceSystem = null!;
    
    public override void _Ready()
    {
        base._Ready();

        _nodeManager.SignalBus.InteractWithSignal += OnInteractWith;
        _nodeManager.SignalBus.GetContextActionsSignal += OnGetContextActions;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        
        // Open the equipment menu when the action is pressed, if possible
        if (@event.IsActionPressed("equipment"))
            TryOpenEquipmentUi();
    }

    /// <summary>
    /// Equip clothing when interacting with an Interactable Clothing object
    /// </summary>
    /// <param name="node"></param>
    /// <param name="args"></param>
    private void OnInteractWith(Node<InteractableComponent> node, ref InteractWithSignal args)
    {
        if (args.Handled)
            return;
        
        if (!_nodeManager.TryGetComponent<ClothingComponent>(node, out var clothingComponent))
            return;

        if (!_nodeManager.TryGetComponent<WearsClothingComponent>(args.Interactee, out var wearsClothingComponent))
            return;

        if (TryEquipClothing((args.Interactee, wearsClothingComponent), (node, clothingComponent)))
            args.Handled = true;
    }
    
    /// <summary>
    /// When right-clicking, creates the context-menu button for equipping items
    /// and adds it to the context menu list
    /// </summary>
    private void OnGetContextActions(Node<InteractableComponent> node, ref GetContextActionsSignal args)
    {
        var button = new Button();
        args.Actions.Add(button);
        button.SetText("Equip Item");
        
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
        
        if (!_nodeManager.TryGetComponent<ClothingComponent>(node, out var clothingComponent))
            return;

        if (node.Owner == wearsClothingComponent.ClothingSlots[clothingComponent.ClothingSlot])
            button.SetText("Unequip Item");

        // Set the button up to equip the item if the user is in-range when pressed
        var interactee = args.Interactee;
        button.Pressed += () =>
        {
            if (!_interactSystem.InRangeUnobstructed(interactee, node.Owner, canInteractComponent.MaxInteractDistance))
                return;

            if (node.Owner != wearsClothingComponent.ClothingSlots[clothingComponent.ClothingSlot])
                TryEquipClothing((interactee, wearsClothingComponent), (node, clothingComponent));
            else
                TryUnequipClothing((interactee, wearsClothingComponent), clothingComponent.ClothingSlot);
        };
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
        clothingSceneSystem.SetDetails((player, wearsClothingComponent));
    }

    public bool TryPutItemInHand(Node<WearsClothingComponent> node, Node<StorableComponent> item)
    {
        // Check if slot exists
        if (!node.Comp.ClothingSlots.TryGetValue(ClothingSlot.Inhand, out var value))
            return false;

        // If hand is occupied, don't do anything
        if (value != null)
            return false;

        if (item.Comp.ItemSize == ItemSize.ExtraLarge)
            return false;
        
        // Clothing is already equipped to a slot and apparently can't be unequipped
        if (_nodeManager.TryGetComponent<ClothingComponent>(item, out var clothingComponent)
            && item.Owner == node.Comp.ClothingSlots[clothingComponent.ClothingSlot]
            && !TryUnequipClothing(node, clothingComponent.ClothingSlot))
            return false;

        // For now, it's likely the item doesn't have an in-hand sprite
        // if equip clothing failed on it
        node.Comp.ClothingSlots[ClothingSlot.Inhand] = item;
        var spriteSlot = node.Owner.GetNodeOrNull<AnimatedSprite2D>("CanvasGroup/Inhand");
        if (spriteSlot != null)
            spriteSlot.SpriteFrames = null;

        if (node.Owner is Node2D node2D && item.Owner is Node2D itemNode2D && itemNode2D.IsVisibleInTree())
        {
            var tween = CreateTween();
            tween.TweenProperty(itemNode2D, "global_position", node2D.GlobalPosition, 0.125f);
            tween.Finished += () =>
            {
                var signal = new ItemPutInHandSignal(item);
                _nodeManager.SignalBus.EmitItemPutInHandSignal(node, ref signal);
            };
            return true;
        }
        
        var signal = new ItemPutInHandSignal(item);
        _nodeManager.SignalBus.EmitItemPutInHandSignal(node, ref signal);
        return true;
    }

    /// <summary>
    /// Try to equip clothing if the slot exists
    /// Set the boolean if you want to try unequipping the slot if it's occupied
    /// </summary>
    public bool TryEquipClothing(Node<WearsClothingComponent> node, Node<ClothingComponent> clothing, bool unequipIfOccupied = false)
    {
        // Check if slot exists
        if (!node.Comp.ClothingSlots.TryGetValue(clothing.Comp.ClothingSlot, out var value))
            return false;
        
        if (value != null && !unequipIfOccupied)
            return false;

        // Try to take off what you're currently wearing
        // If it fails, it means you can't take off whatever you're currently wearing
        // i.e. some kind of cursed or binded object
        if (value != null && unequipIfOccupied && !TryUnequipClothing(node, clothing.Comp.ClothingSlot))
            return false;

        if (clothing.Owner == node.Comp.ClothingSlots[ClothingSlot.Inhand] && !TryUnequipClothing(node, ClothingSlot.Inhand))
            return false;
        
        // Equip the clothing to that slot
        node.Comp.ClothingSlots[clothing.Comp.ClothingSlot] = clothing;
        
        // Assign the spriteFrame to the correct AnimatedSprite2D node
        // This causes the item to show up on the character's body
        var spriteSlot = node.Owner.GetNodeOrNull<AnimatedSprite2D>("CanvasGroup/" + clothing.Comp.ClothingSlot);
        if (clothing.Comp.EquippedSpriteFrames != null && spriteSlot != null)
            spriteSlot.SpriteFrames = clothing.Comp.EquippedSpriteFrames;

        var signal = new ClothingEquippedSignal(clothing);
        _nodeManager.SignalBus.EmitClothingEquippedSignal(node, ref signal);
        return true;
    }

    public bool TryUnequipClothing(Node<WearsClothingComponent> node, ClothingSlot slot)
    {
        // Check if slot exists
        if (!node.Comp.ClothingSlots.TryGetValue(slot, out var value))
            return false;
        
        // Nothing to unequip
        if (value == null)
            return false;
        
        // Put the item back into the world and make it visible again
        value.Reparent(node.Owner.GetParent());
        if (value is Node2D clothesNode2D)
            clothesNode2D.SetVisible(true);
        
        // TODO: maybe some kind of CursedClothingComponent or something to make perma-equipped clothes that require dispelling
        // Removes the item from the character's clothing slots and removes the appearance of it on the character
        node.Comp.ClothingSlots[slot] = null;
        var spriteSlot = node.Owner.GetNodeOrNull<AnimatedSprite2D>("CanvasGroup/" + slot);
        if (spriteSlot != null)
            spriteSlot.SpriteFrames = null;

        if (slot == ClothingSlot.Inhand && _nodeManager.TryGetComponent<StorableComponent>(value, out var storableComponent))
        {
            var signal = new ItemRemovedFromHandSignal((value, storableComponent));
            _nodeManager.SignalBus.EmitItemRemovedFromHandSignal(node, ref signal);
            return true;
        }
        
        // TODO: Now put the item back into their inventory, their hand, drop it into the world, literally anything
        if (_nodeManager.TryGetComponent<ClothingComponent>(value, out var clothingComponent))
        {
            var signal = new ClothingUnequippedSignal((value, clothingComponent));
            _nodeManager.SignalBus.EmitClothingUnequippedSignal(node, ref signal);
        }

        return true;
    }

    public bool IsHandEmpty(Node<WearsClothingComponent> node)
    {
        return node.Comp.ClothingSlots[ClothingSlot.Inhand] == null;
    }
}

public partial class ClothingEquippedSignal : UserSignalArgs
{
    public Node<ClothingComponent> Clothing;

    public ClothingEquippedSignal(Node<ClothingComponent> clothing)
    {
        Clothing = clothing;
    }
}

public partial class ItemPutInHandSignal : UserSignalArgs
{
    public Node<StorableComponent> Storable;

    public ItemPutInHandSignal(Node<StorableComponent> storable)
    {
        Storable = storable;
    }
}

public partial class ClothingUnequippedSignal : UserSignalArgs
{
    public Node<ClothingComponent> Clothing;

    public ClothingUnequippedSignal(Node<ClothingComponent> clothing)
    {
        Clothing = clothing;
    }
}

public partial class ItemRemovedFromHandSignal : UserSignalArgs
{
    public Node<StorableComponent> Storable;

    public ItemRemovedFromHandSignal(Node<StorableComponent> storable)
    {
        Storable = storable;
    }
}