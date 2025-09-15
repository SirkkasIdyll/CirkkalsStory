using CS.Components.Clothing;
using CS.Components.Description;
using CS.Components.Interaction;
using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Nodes.Scenes.Inventory;

public partial class ClothingSceneSystem : GridContainer
{
	[ExportCategory("Owned")]
	[Export] private Button _title = null!;
	[Export] private AtlasTexture _texture = null!;
	[Export] private Dictionary<ClothingSlot, TextureRect> _clothingSlots = null!; 
	
	private readonly NodeSystemManager _nodeSystemManager = NodeSystemManager.Instance;
	private readonly NodeManager _nodeManager = NodeManager.Instance;
	[InjectDependency] private readonly ClothingSystem _clothingSystem = null!;
	[InjectDependency] private readonly DescriptionSystem _descriptionSystem = null!;
	[InjectDependency] private readonly InteractSystem _interactSystem = null!;
	
	private Node? Character;

	public override void _Ready()
	{
		base._Ready();

		_nodeManager.SignalBus.ItemPutInHandSignal += OnItemPutInHand;
		_nodeManager.SignalBus.ClothingEquippedSignal += OnEquipped;
		_nodeManager.SignalBus.ClothingUnequippedSignal += OnUnequipped;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		_nodeManager.SignalBus.ItemPutInHandSignal -= OnItemPutInHand;
		_nodeManager.SignalBus.ClothingEquippedSignal -= OnEquipped;
		_nodeManager.SignalBus.ClothingUnequippedSignal -= OnUnequipped;
	}

	private void OnItemPutInHand(Node<WearsClothingComponent> node, ref ItemPutInHandSignal args)
	{
		if (node.Owner != Character)
			return;
		
		// Set sprite to clothing's icon
		if (!_descriptionSystem.TryGetSprite(args.Storable, out var sprite2D))
			return;

		if (!_clothingSlots.TryGetValue(ClothingSlot.Inhand, out var textureRect))
			return;
		
		textureRect.Texture = sprite2D.Texture;
	}

	private void OnEquipped(Node<WearsClothingComponent> node, ref ClothingEquippedSignal args)
	{
		if (node.Owner != Character)
			return;
		
		// Set sprite to clothing's icon
		if (!_descriptionSystem.TryGetSprite(args.Clothing, out var sprite2D))
			return;

		if (!_clothingSlots.TryGetValue(args.Clothing.Comp.ClothingSlot, out var textureRect))
			return;
		
		textureRect.Texture = sprite2D.Texture;
	}

	private void OnUnequipped(Node<WearsClothingComponent> node, ref ClothingUnequippedSignal args)
	{
		if (node.Owner != Character)
			return;
		
		if (!_clothingSlots.TryGetValue(args.Clothing.Comp.ClothingSlot, out var textureRect))
			return;
		
		// Set sprite to generic atlas icon
		var atlasTexture = (AtlasTexture) _texture.Duplicate();
		atlasTexture.Region = new Rect2(new Vector2(0, 32 * (int)args.Clothing.Comp.ClothingSlot), new Vector2(32, 32));
		textureRect.Texture = atlasTexture;
	}

	public void SetDetails(Node<WearsClothingComponent> node)
	{
		_nodeSystemManager.InjectNodeSystemDependencies(this);

		if (_descriptionSystem.TryGetDisplayName(node, out var name))
			_title.SetText(name + "'s Equipment");

		GD.Print(_clothingSlots);
		foreach (var (clothingSlot, textureRect) in _clothingSlots)
		{
			GD.Print(_clothingSlots[clothingSlot]);
			GD.Print(_clothingSlots[ClothingSlot.Shoes]);
			GD.Print(clothingSlot);
			GD.Print(textureRect);
		}
		
		foreach (var (clothingSlot, clothingItem) in node.Comp.ClothingSlots)
		{
			var clothingSlotIndex = (int)clothingSlot;
			// When the particular slot's image is clicked, try to unequip the clothing in that slot.
			// It's fine if there is an attempt to unequip nothing as that will be handled in the unequip function
			if (_clothingSlots.TryGetValue(clothingSlot, out var textureRect))
			{
				textureRect.GuiInput += @event =>
				{
					if (@event is InputEventMouseButton eventButton && eventButton.ButtonIndex == MouseButton.Left && eventButton.Pressed)
						_clothingSystem.TryUnequipClothing(node, clothingSlot);
				};
			}

			// No clothing equipped in this slot, set it to the generic icon and continue
			if (clothingItem == null)
			{
				var atlasTexture = (AtlasTexture) _texture.Duplicate();
				atlasTexture.Region = new Rect2(new Vector2(0, 32 * clothingSlotIndex), new Vector2(32, 32));
				_clothingSlots[clothingSlot].Texture = atlasTexture;
				continue;
			}
			
			// Set the texture in the equipment UI scene to the texture of the clothing item's icon (not the worn sprite)
			if (_descriptionSystem.TryGetSprite(node, out var sprite2D))
				_clothingSlots[clothingSlot].Texture = sprite2D.Texture;
		}
	}
}