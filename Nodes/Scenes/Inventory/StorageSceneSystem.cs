using CS.Components.Clothing;
using CS.Components.Description;
using CS.Components.Interaction;
using CS.Components.Inventory;
using CS.Components.Player;
using CS.Nodes.UI.ContextButtonList;
using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Nodes.Scenes.Inventory;

public partial class StorageSceneSystem : VBoxContainer
{
	[ExportCategory("Owned")]
	[Export] private ProgressBar _storageProgressBar = null!;
	[Export] private Label _storageProgressBarLabel = null!;
	[Export] private VBoxContainer _itemButtonContainer = null!;
	
	private readonly NodeSystemManager _nodeSystemManager = NodeSystemManager.Instance;
	private readonly NodeManager _nodeManager = NodeManager.Instance;
	[InjectDependency] private readonly ClothingSystem _clothingSystem = null!;
	[InjectDependency] private readonly DescriptionSystem _descriptionSystem = null!;
	[InjectDependency] private readonly PlayerManagerSystem _playerManagerSystem = null!;
	[InjectDependency] private readonly StorageSystem _storageSystem = null!;
	
	private PackedScene _contextButtonList = ResourceLoader.Load<PackedScene>("res://Nodes/UI/ContextButtonList/ContextButtonList.tscn");
	private const string Green = "#5c9055";
	private const string Yellow = "#dfcb43";
	private const string Red = "#c22e15";

	private Dictionary<Node, Button> _buttonDictionary = [];

	public override void _Ready()
	{
		base._Ready();

		_nodeManager.SignalBus.ItemPutInStorageSignal += OnItemPutInStorage;
		_nodeManager.SignalBus.ItemRemovedFromStorageSignal += OnItemRemovedFromStorage;
	}

	private void OnItemPutInStorage(Node<StorageComponent> node, ref ItemPutInStorageSignal args)
	{
		var button = CreateItemButton(node, args.Storable);
		_itemButtonContainer.AddChild(button);
		_buttonDictionary[args.Storable] = button;
		UpdateStorageProgressBar(node);
	}

	private void OnItemRemovedFromStorage(Node<StorageComponent> node, ref ItemRemovedFromStorageSignal args)
	{
		var button = _buttonDictionary[args.Storable];
		_itemButtonContainer.RemoveChild(button);
		_buttonDictionary.Remove(args.Storable);
		UpdateStorageProgressBar(node);
	}

	private void OnPrimaryInteract(Node<StorageComponent> node, Node<StorableComponent> item)
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
	}

	private void OnSecondaryInteract(Node<StorageComponent> node, Node<StorableComponent> item)
	{
		var player = _playerManagerSystem.TryGetPlayer();
		if (player == null)
			return;
		
		if (!_nodeManager.TryGetComponent<InteractableComponent>(item, out var interactableComponent))
			return;
		
		var signal = new GetContextActionsSignal(player);
		_nodeManager.SignalBus.EmitGetContextActionsSignal((item, interactableComponent), ref signal);
		
		var nodeButtonList = _contextButtonList.Instantiate<ContextButtonListSystem>();
		nodeButtonList.Setup(signal.Actions);
		_storageSystem.GetParent().GetNode<CanvasLayer>("CanvasLayer").AddChild(nodeButtonList);
		nodeButtonList.SetPosition(GetViewport().GetMousePosition());
	}

	public void SetDetails(Node<StorageComponent> node)
	{
		_nodeSystemManager.InjectNodeSystemDependencies(this);
		UpdateStorageProgressBar(node);

		var items = _storageSystem.GetStorageItems(node);
		AddItemButtons(node, items);
	}

	private void UpdateStorageProgressBar(Node<StorageComponent> node)
	{
		_storageProgressBarLabel.SetText(node.Comp.TotalStoredSpace + " / " + node.Comp.MaxSpace);
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

	private void AddItemButtons(Node<StorageComponent> node, Array<Node> items)
	{
		foreach (var item in items)
		{
			if (!_nodeManager.TryGetComponent<StorableComponent>(item, out var storableComponent))
				continue;

			var button = CreateItemButton(node, (item, storableComponent));
			_itemButtonContainer.AddChild(button);
			_buttonDictionary[item] = button;
		}
	}

	private Button CreateItemButton(Node<StorageComponent> node, Node<StorableComponent> item)
	{
		var button = new Button();
		button.SetThemeTypeVariation("ButtonSmall");
		
		if (_descriptionSystem.TryGetDisplayName(item,  out var displayName))
			button.SetText(displayName);

		if (_descriptionSystem.TryGetSprite(item, out var sprite))
			button.Icon = sprite.Texture;

		button.GuiInput += @event =>
		{
			if (@event.IsActionPressed("primary_interact"))
				OnPrimaryInteract(node, item);

			if (@event.IsActionPressed("secondary_interact"))
				OnSecondaryInteract(node, item);
		};
		return button;
	}
}