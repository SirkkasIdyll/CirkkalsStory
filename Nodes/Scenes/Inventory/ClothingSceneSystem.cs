using CS.Components.Clothing;
using CS.Components.Description;
using CS.Components.Interaction;
using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Nodes.Scenes.Inventory;

/// <summary>
/// Displays the <see cref="ClothingComponent"/> worn.
/// Used for equipping and unequipping clothes.
/// </summary>
public partial class ClothingSceneSystem : GridContainer, IModifiableScene
{
	[ExportCategory("Owned")]
	[Export] private Button _title = null!;
	[Export] private AtlasTexture _texture = null!;
	[Export] private Dictionary<ClothingSlot, ClothingTextureRect> _clothingSlots = [];

	private readonly NodeManager _nodeManager = NodeManager.Instance;
	private readonly NodeSystemManager _nodeSystemManager = NodeSystemManager.Instance;
	[InjectDependency] private readonly ClothingSystem _clothingSystem = null!;
	[InjectDependency] private readonly DescriptionSystem _descriptionSystem = null!;
	
	private Node<WearsClothingComponent>? _uiOwner;

	public override void _ExitTree()
	{
		base._ExitTree();
		
		_nodeManager.SignalBus.ClothingEquippedSignal -= OnClothingEquipped;
		_nodeManager.SignalBus.ClothingUnequippedSignal -= OnClothingUnequipped;
		_nodeManager.SignalBus.ItemPutInHandSignal -= OnItemPutInHand;
		_nodeManager.SignalBus.ItemRemovedFromHandSignal -= OnItemRemovedFromHand;
	}

	public override void _Ready()
	{
		base._Ready();
		_nodeSystemManager.InjectNodeSystemDependencies(this);

		_nodeManager.SignalBus.ClothingEquippedSignal += OnClothingEquipped;
		_nodeManager.SignalBus.ClothingUnequippedSignal += OnClothingUnequipped;
		_nodeManager.SignalBus.ItemPutInHandSignal += OnItemPutInHand;
		_nodeManager.SignalBus.ItemRemovedFromHandSignal += OnItemRemovedFromHand;
	}
	
	public void ModifyScene(Node node)
	{
		if (!_nodeManager.TryGetComponent<WearsClothingComponent>(node, out var wearsClothingComponent))
			return;

		_uiOwner = (node, wearsClothingComponent);
		
		if (_descriptionSystem.TryGetDisplayName(node, out var name))
			_title.SetText(name + "'s Equipment");
		
		foreach (var (clothingSlot, clothingItem) in _uiOwner.Value.Comp.ClothingSlots)
		{
			var clothingSlotIndex = (int)clothingSlot;
			// When the particular slot's image is clicked, try to unequip the clothing in that slot.
			// It's fine if there is an attempt to unequip nothing as that will be handled in the unequip function
			if (_clothingSlots.TryGetValue(clothingSlot, out var clothingTextureRect))
			{
				clothingTextureRect.Item = clothingItem;
				clothingTextureRect.Slot = clothingSlot;
				
				clothingTextureRect.GuiInput += @event =>
				{
					/*if (@event.IsActionPressed("primary_interact"))
						OnPrimaryInteract(node, clothingSlot);*/

					if (@event.IsActionPressed("secondary_interact"))
						OnSecondayInteract(_uiOwner.Value, clothingSlot);
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
			if (_descriptionSystem.TryGetSprite(clothingItem, out var sprite2D))
				_clothingSlots[clothingSlot].Texture = sprite2D.Texture;
		}
	}

	private void OnClothingEquipped(Node<WearsClothingComponent> node, ref ClothingEquippedSignal args)
	{
		if (_uiOwner == null || node.Owner != _uiOwner.Value.Owner)
			return;
		
		// Set sprite to clothing's icon
		if (!_descriptionSystem.TryGetSprite(args.Clothing, out var sprite2D))
			return;

		if (!_clothingSlots.TryGetValue(args.Clothing.Comp.ClothingSlot, out var clothingTextureRect))
			return;
		
		clothingTextureRect.Texture = sprite2D.Texture;
		clothingTextureRect.Item = args.Clothing;
	}

	private void OnClothingUnequipped(Node<WearsClothingComponent> node, ref ClothingUnequippedSignal args)
	{
		if (_uiOwner == null || node.Owner != _uiOwner.Value.Owner)
			return;
		
		if (!_clothingSlots.TryGetValue(args.Clothing.Comp.ClothingSlot, out var clothingTextureRect))
			return;
		
		// Set sprite to generic atlas icon
		var atlasTexture = (AtlasTexture) _texture.Duplicate();
		atlasTexture.Region = new Rect2(new Vector2(0, 32 * (int)args.Clothing.Comp.ClothingSlot), new Vector2(32, 32));
		clothingTextureRect.Texture = atlasTexture;
		clothingTextureRect.Item = null;
	}

	private void OnItemPutInHand(Node<WearsClothingComponent> node, ref ItemPutInHandSignal args)
	{
		if (_uiOwner == null || node.Owner != _uiOwner.Value.Owner)
			return;
		
		// Set sprite to clothing's icon
		if (!_descriptionSystem.TryGetSprite(args.Storable, out var sprite2D))
			return;

		if (!_clothingSlots.TryGetValue(ClothingSlot.Inhand, out var clothingTextureRect))
			return;
		
		clothingTextureRect.Texture = sprite2D.Texture;
		clothingTextureRect.Item = args.Storable;
	}

	private void OnItemRemovedFromHand(Node<WearsClothingComponent> node, ref ItemRemovedFromHandSignal args)
	{
		if (_uiOwner == null || node.Owner != _uiOwner.Value.Owner)
			return;
		
		if (!_clothingSlots.TryGetValue(ClothingSlot.Inhand, out var clothingTextureRect))
			return;
		
		// Set sprite to generic atlas icon
		var atlasTexture = (AtlasTexture) _texture.Duplicate();
		atlasTexture.Region = new Rect2(new Vector2(0, 32 * (int)ClothingSlot.Inhand), new Vector2(32, 32));
		clothingTextureRect.Texture = atlasTexture;
		clothingTextureRect.Item = null;
	}

	/// <summary>
	/// When clicking on an equipped piece of clothing, attempts to put the item in hand
	/// When clicking an empty space with a piece of clothing in hand, attempts to equip the clothing to that slot
	/// </summary>
	/*private void OnPrimaryInteract(Node<WearsClothingComponent> node, ClothingSlot clothingSlot)
	{
		var clothingItem = node.Comp.ClothingSlots[clothingSlot];
		// If the user clicks on an empty slot, with a piece of clothing in-hand,
		// try to equip the clothing item to that slot.
		if (clothingItem == null)
		{
			var inHandItem = node.Comp.ClothingSlots[ClothingSlot.Inhand];
			if (inHandItem == null)
				return;

			if (!_nodeManager.TryGetComponent<ClothingComponent>(inHandItem, out var clothingComponent))
				return;

			if (clothingComponent.ClothingSlot != clothingSlot)
				return;
			
			_clothingSystem.TryEquipClothing(node, (inHandItem, clothingComponent), true);
			return;
		}

		// If the user clicks on a piece of clothing,
		// attempt to put the item in hand
		if (!_nodeManager.TryGetComponent<StorableComponent>(clothingItem, out var storableComponent))
			return;
		
		_clothingSystem.TryPutItemInHand(node, (clothingItem, storableComponent));
	}*/

	private void OnSecondayInteract(Node<WearsClothingComponent> node, ClothingSlot clothingSlot)
	{
		var clothingItem = node.Comp.ClothingSlots[clothingSlot];
		if (clothingItem == null)
			return;

		if (!_nodeManager.TryGetComponent<InteractableComponent>(clothingItem, out var interactableComponent))
			return;
		
		var signal = new GetContextActionsSignal(node);
		_nodeManager.SignalBus.EmitGetContextActionsSignal((clothingItem, interactableComponent), ref signal);
		_clothingSystem.GetParent().GetNode<CanvasLayer>("CanvasLayer").AddChild(signal.ContextMenu);
	}
}