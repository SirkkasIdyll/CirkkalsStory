using CS.Components.Clothing;
using CS.Components.Description;
using CS.Components.Interaction;
using CS.Components.Inventory;
using CS.Components.Player;
using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Nodes.Scenes.Inventory;

/// <summary>
/// Related to the <see cref="StorageComponent"/>
/// </summary>
public partial class StorageSceneSystem : VBoxContainer, IModifiableScene
{
	[ExportCategory("Owned")]
	[Export] private Button? _title;
	[Export] private ProgressBar _storageProgressBar = null!;
	[Export] private Label _storageProgressBarLabel = null!;
	[Export] private VBoxContainer _itemButtonContainer = null!;
	
	private readonly NodeManager _nodeManager = NodeManager.Instance;
	private readonly NodeSystemManager _nodeSystemManager = NodeSystemManager.Instance;
	[InjectDependency] private readonly ClothingSystem _clothingSystem = null!;
	[InjectDependency] private readonly DescriptionSystem _descriptionSystem = null!;
	[InjectDependency] private readonly PlayerManagerSystem _playerManagerSystem = null!;
	[InjectDependency] private readonly StorageSystem _storageSystem = null!;

	private const string Green = "#5c9055";
	private const string Yellow = "#dfcb43";
	private const string Red = "#c22e15";
	private Dictionary<Node, Button> _buttonDictionary = [];
	
	private Node? _uiOwner;
	private StorageComponent? _uiStorageComponent;

	/// <summary>
	/// Checks if something has a <see cref="StorableComponent"/>
	/// and can fit inside the storage being dragged over
	/// </summary>
	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		var node = (Node?)data;
		
		if (node == null)
			return false;

		if (!_nodeManager.TryGetComponent<StorableComponent>(node, out var storable))
			return false;

		if (_uiOwner == null || _uiStorageComponent == null)
			return false;

		if (_storageSystem.CanBeAddedToStorage((_uiOwner, _uiStorageComponent), (node, storable)))
			return true;
		
		return false;
	}

	/// <summary>
	/// Attempts to store a <see cref="StorableComponent"/> in a <see cref="StorageComponent"/>
	/// when drag is dropped
	/// </summary>
	public override void _DropData(Vector2 atPosition, Variant data)
	{
		var node = (Node)data;

		if (!_nodeManager.TryGetComponent<StorableComponent>(node, out var storable))
			return;

		if (_uiOwner == null || _uiStorageComponent == null)
			return;

		_storageSystem.TryAddItemToStorage((_uiOwner, _uiStorageComponent), (node, storable));
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		_nodeManager.SignalBus.ItemPutInStorageSignal -= OnItemPutInStorage;
		_nodeManager.SignalBus.ItemRemovedFromStorageSignal -= OnItemRemovedFromStorage;
	}

	public override void _Ready()
	{
		base._Ready();
		_nodeSystemManager.InjectNodeSystemDependencies(this);

		MouseEntered += OnMouseEntered;
		MouseExited += OnMouseExited;

		_nodeManager.SignalBus.ItemPutInStorageSignal += OnItemPutInStorage;
		_nodeManager.SignalBus.ItemRemovedFromStorageSignal += OnItemRemovedFromStorage;
	}
	
	/// <summary>
	/// When a <see cref="StorableComponent"/> is put in a <see cref="StorageComponent"/>,
	/// create the UI button for that item in the storage UI
	/// </summary>
	private void OnItemPutInStorage(Node<StorageComponent> node, ref ItemPutInStorageSignal args)
	{
		if (node.Owner != _uiOwner)
			return;
		
		AddItemButtons(node, [args.Storable]);
		UpdateStorageProgressBar(node);
	}

	/// <summary>
	/// When a <see cref="StorableComponent"/> is removed from a <see cref="StorageComponent"/>,
	/// remove the UI button for that item in the storage UI
	/// </summary>
	private void OnItemRemovedFromStorage(Node<StorageComponent> node, ref ItemRemovedFromStorageSignal args)
	{
		if (node.Owner != _uiOwner)
			return;
		
		RemoveItemButtons(node, [args.Storable]);
		UpdateStorageProgressBar(node);
	}

	/*private void OnPrimaryInteract(Node<StorageComponent> node, Node<StorableComponent> item)
	{
		var player = _playerManagerSystem.TryGetPlayer();
		if (player == null)
			return;

		if (!_nodeManager.TryGetComponent<WearsClothingComponent>(player, out var wearsClothingComponent))
			return;

		if (!_clothingSystem.IsHandEmpty((player, wearsClothingComponent)))
			return;

		if (!_storageSystem.TryRemoveItemFromStorage(node, item, out var removedItem))
			return;
		
		// Shouldn't really be any reason this won't work at this point
		_clothingSystem.TryPutItemInHand((player, wearsClothingComponent), item);
	}*/

	/// <summary>
	/// Open context menu when right-clicking on a <see cref="StorageComponent"/>
	/// </summary>
	private void OnSecondaryInteract(Node<StorageComponent> node, Node<StorableComponent> item)
	{
		var player = _playerManagerSystem.TryGetPlayer();
		if (player == null)
			return;
		
		if (!_nodeManager.TryGetComponent<InteractableComponent>(item, out var interactableComponent))
			return;
		
		var signal = new GetContextActionsSignal(player);
		_nodeManager.SignalBus.EmitGetContextActionsSignal((item, interactableComponent), ref signal);
		_storageSystem.GetParent().GetNode<CanvasLayer>("CanvasLayer").AddChild(signal.ContextMenu);
	}
	
	/// <summary>
	/// Sets the title to the name of the item
	/// Adds a SFX when the storage is closed
	/// Initializes the storage weight display
	/// Creates buttons for the items inside the storage
	/// And sends a signal that we've opened the storage
	/// </summary>
	public void ModifyScene(Node node)
	{
		_uiOwner = node;

		if (!_nodeManager.TryGetComponent<StorageComponent>(_uiOwner, out var storageComponent))
			return;
		
		_uiStorageComponent = storageComponent;
		
		if (_title != null && _descriptionSystem.TryGetDisplayName(_uiOwner, out var name))
			_title.SetText(name);
		
		// Play closing storage SFX
		TreeExiting += () =>
		{
			var signal = new StorageClosedSignal();
			_nodeManager.SignalBus.EmitStorageClosedSignal((_uiOwner, _uiStorageComponent), ref signal);
		};
		
		UpdateStorageProgressBar((_uiOwner, _uiStorageComponent));
		var items = _storageSystem.GetStorageItems((_uiOwner, _uiStorageComponent));
		AddItemButtons((_uiOwner, _uiStorageComponent), items);

		var signal = new StorageOpenedSignal();
		_nodeManager.SignalBus.EmitStorageOpenedSignal((_uiOwner, _uiStorageComponent), ref signal);
	}

	/// <summary>
	/// Creates interactable buttons for each item inside a storage
	/// </summary>
	private void AddItemButtons(Node<StorageComponent> node, Array<Node> items)
	{
		foreach (var item in items)
		{
			if (!_nodeManager.TryGetComponent<StorableComponent>(item, out var storableComponent))
				continue;

			var button = new StorageItemButton(item);
			button.SetThemeTypeVariation("ButtonSmall");
		
			if (_descriptionSystem.TryGetDisplayName(item,  out var displayName))
				button.SetText(displayName);

			if (_descriptionSystem.TryGetSprite(item, out var sprite))
				button.Icon = sprite.Texture;

			button.GuiInput += @event =>
			{
				/*if (@event.IsActionPressed("primary_interact"))
					OnPrimaryInteract(node, item);*/

				if (@event.IsActionPressed("secondary_interact"))
					OnSecondaryInteract(node, (item, storableComponent));
			}; 
			
			_itemButtonContainer.AddChild(button);
			_buttonDictionary[item] = button;
		}
	}

	private void RemoveItemButtons(Node<StorageComponent> node, Array<Node> items)
	{
		foreach (var item in items)
		{
			_itemButtonContainer.RemoveChild(_buttonDictionary[item]);
			_buttonDictionary.Remove(item);
		}
	}

	private void OnMouseEntered()
	{
		
	}

	private void OnMouseExited()
	{
		
	}

	private void UpdateStorageProgressBar(Node<StorageComponent> node)
	{
		_storageProgressBarLabel.SetText(float.Round(node.Comp.VolumeOccupied, 2)
		                                 + " / " + float.Round(node.Comp.Capacity, 2));
		var storagePercentage = _storageSystem.GetStoragePercentage(node);
		_storageProgressBar.Value = storagePercentage;
		
		// Sets the color to green, red, or yellow depending on how filled the storage is
		var storageBarColor = new StyleBoxFlat();
		if (storagePercentage > 80)
			storageBarColor.BgColor = Color.FromHtml(Red);
		else if (storagePercentage > 50)
			storageBarColor.BgColor = Color.FromHtml(Yellow);
		else
			storageBarColor.BgColor = Color.FromHtml(Green);
		_storageProgressBar.AddThemeStyleboxOverride("fill", storageBarColor);
	}
}