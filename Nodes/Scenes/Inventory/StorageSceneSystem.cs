using CS.Components.Description;
using CS.Components.Inventory;
using CS.SlimeFactory;
using Godot;

namespace CS.Nodes.Scenes.Inventory;

public partial class StorageSceneSystem : VBoxContainer
{
	[ExportCategory("Owned")]
	[Export] private ProgressBar _storageProgressBar = null!;
	
	private readonly NodeSystemManager _nodeSystemManager = NodeSystemManager.Instance;
	private readonly NodeManager _nodeManager = NodeManager.Instance;
	[InjectDependency] private readonly DescriptionSystem _descriptionSystem = null!;
	[InjectDependency] private readonly StorageSystem _storageSystem = null!;
	
	private const string Green = "#5c9055";
	private const string Yellow = "#dfcb43";
	private const string Red = "#c22e15";

	public void SetDetails(Node<StorageComponent> node)
	{
		_nodeSystemManager.InjectNodeSystemDependencies(this);
		UpdateStorageProgressBar(node);

		var items = _storageSystem.GetStorageItems(node);
		foreach (var item in items)
			AddItemButton(item);
	}

	private void UpdateStorageProgressBar(Node<StorageComponent> node)
	{
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

	private void AddItemButton(Node item)
	{
		var marginContainer = new MarginContainer();
		marginContainer.SetThemeTypeVariation("MarginContainerXSmall");

		var button = new Button();
		button.SetThemeTypeVariation("ButtonSmall");
		
		if (_descriptionSystem.TryGetDisplayName(item,  out var displayName))
			button.SetText(displayName);

		if (_descriptionSystem.TryGetSprite(item, out var sprite))
			button.Icon = sprite.Texture;
		
		AddChild(marginContainer);
		marginContainer.AddChild(button);
	}
}