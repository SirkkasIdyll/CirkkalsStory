﻿using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Components.Clothing;

public partial class WearsClothingComponent : Component
{
    /// <summary>
    /// The clothing slots that something that can wear clothing can accomodate
    ///
    /// I guess you could possibly just allow deleting certain slots
    /// to prevent equipping of certain slots
    /// for special species
    /// </summary>
    public Dictionary<ClothingSlot, Node?> ClothingSlots = new()
    {
        { ClothingSlot.Shoes, null },
        { ClothingSlot.Bottom, null },
        { ClothingSlot.Top, null },
        { ClothingSlot.Belt, null },
        { ClothingSlot.Outerwear, null },
        { ClothingSlot.Neck, null },
        { ClothingSlot.Inhand, null },
        { ClothingSlot.Face, null },
        { ClothingSlot.Hat, null }
    };
}