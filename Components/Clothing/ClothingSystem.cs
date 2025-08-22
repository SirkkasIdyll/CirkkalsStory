using CS.Components.Interaction;
using CS.SlimeFactory;
using Godot;

namespace CS.Components.Clothing;

public partial class ClothingSystem : NodeSystem
{
    [InjectDependency] private readonly InteractSystem _interactSystem = null!;
    
    public override void _Ready()
    {
        base._Ready();

        _nodeManager.SignalBus.InteractWithSignal += OnInteractWith;
        _nodeManager.SignalBus.GetContextActionsSignal += OnGetContextActions;
    }

    private void OnInteractWith(Node<InteractableComponent> node, ref InteractWithSignal args)
    {
        if (!_nodeManager.TryGetComponent<ClothingComponent>(node, out var clothingComponent))
            return;

        if (!_nodeManager.TryGetComponent<WearsClothingComponent>(args.Interactee, out var wearsClothingComponent))
            return;

        TryEquipClothing((args.Interactee, wearsClothingComponent), (node, clothingComponent));
    }
    
    private void OnGetContextActions(Node<InteractableComponent> node, ref GetContextActionsSignal args)
    {
        var button = new Button();
        args.Actions.Add(button);
        button.Text = "Equip item";

        if (!_nodeManager.TryGetComponent<CanInteractComponent>(args.Interactee, out var canInteractComponent))
        {
            button.Disabled = true;
            return;
        }

        var interactee = args.Interactee;
        button.Pressed += () =>
        {
            if (!_interactSystem.InRangeUnobstructed(interactee, node.Owner, canInteractComponent.MaxInteractDistance))
                return;
            
            if (!_nodeManager.TryGetComponent<ClothingComponent>(node, out var clothingComponent))
                return;

            if (!_nodeManager.TryGetComponent<WearsClothingComponent>(interactee, out var wearsClothingComponent))
                return;
            
            TryEquipClothing((interactee, wearsClothingComponent), (node, clothingComponent));
        };
    }

    public bool TryEquipClothing(Node<WearsClothingComponent> node, Node<ClothingComponent> clothing)
    {
        // Slot doesn't exist
        if (!node.Comp.ClothingSlots.TryGetValue(clothing.Comp.ClothingSlot, out var value))
            return false;

        // Try to take off what you're currently wearing
        // If it fails, it means you can't take off whatever you're currently wearing
        // i.e. some kind of cursed or binded object
        if (value != null && !TryUnequipClothing(node, clothing.Comp.ClothingSlot))
            return false;
        
        // Equip the clothing to that slot
        node.Comp.ClothingSlots[clothing.Comp.ClothingSlot] = clothing;
        // Assign the spriteFrame to the correct AnimatedSprite2D node
        var spriteSlot = node.Owner.GetNodeOrNull<AnimatedSprite2D>("CanvasGroup/" + clothing.Comp.ClothingSlot);
        if (clothing.Comp.EquippedSpriteFrames != null && spriteSlot != null)
            spriteSlot.SpriteFrames = clothing.Comp.EquippedSpriteFrames;

        // Attach the clothing to the wearer and have it follow them around invisibly
        if (node.Owner is Node2D wearerNode2D && clothing.Owner is Node2D clothingNode2D)
        {
            clothingNode2D.SetVisible(false);
            clothing.Owner.Reparent(node.Owner, false);
            clothingNode2D.GlobalPosition = wearerNode2D.GlobalPosition;
        }

        return true;
    }

    public bool TryUnequipClothing(Node<WearsClothingComponent> node, ClothingSlot slot)
    {
        // Slot doesn't exist
        if (!node.Comp.ClothingSlots.TryGetValue(slot, out var value))
            return false;
        
        // TODO: maybe some kind of CursedClothingComponent or something to make perma-equipped clothes that require dispelling
        
        // TODO: Put the item back into their inventory, their hand, drop it into the world, literally anything
        node.Comp.ClothingSlots[slot] = null;
        var spriteSlot = node.Owner.GetNodeOrNull<AnimatedSprite2D>("CanvasGroup/" + slot);
        if (spriteSlot != null)
            spriteSlot.SpriteFrames = null;

        return true;
    }
}