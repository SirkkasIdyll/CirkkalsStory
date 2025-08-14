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

        // Take off what you're currently wearing and place it wherever else
        if (value != null)
            TryUnequipClothing(node, clothing.Comp.ClothingSlot);
        
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

        // Nothing equipped
        if (value == null)
            return false;
        
        // TODO: Put the item back into their inventory, their hand, drop it into the world, literally anything
        node.Comp.ClothingSlots[slot] = null;
        
        var spriteSlot = node.Owner.GetNodeOrNull<AnimatedSprite2D>("CanvasGroup/" + slot);
        if (spriteSlot != null)
            spriteSlot.SpriteFrames = null;

        return true;
    }
}