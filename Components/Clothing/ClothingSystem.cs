using CS.SlimeFactory;
using Godot;

namespace CS.Components.Clothing;

public partial class ClothingSystem : NodeSystem
{
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
        
        node.Comp.ClothingSlots[clothing.Comp.ClothingSlot] = value;
        var spriteSlot = node.Owner.GetNodeOrNull<AnimatedSprite2D>("CanvasGroup/" + clothing.Comp.ClothingSlot);
        if (clothing.Comp.ClothingSprite != null && spriteSlot != null)
            spriteSlot.SpriteFrames = clothing.Comp.ClothingSprite.SpriteFrames;

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