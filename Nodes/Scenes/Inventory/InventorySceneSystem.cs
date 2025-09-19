using CS.Components.Clothing;
using CS.Components.Description;
using CS.Components.Inventory;
using CS.Components.Player;
using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Nodes.Scenes.Inventory;

public partial class InventorySceneSystem : VBoxContainer
{
    [ExportCategory("Owned")]
    [Export] private Button _title = null!;

    private readonly NodeManager _nodeManager = NodeManager.Instance;
    private readonly NodeSystemManager _nodeSystemManager = NodeSystemManager.Instance;
    [InjectDependency] private readonly ClothingSystem _clothingSystem = null!;
    [InjectDependency] private readonly DescriptionSystem _descriptionSystem = null!;
    [InjectDependency] private readonly PlayerManagerSystem _playerManagerSystem = null!;
    [InjectDependency] private readonly StorageSystem _storageSystem = null!;

    private PackedScene _storageFragmentScene =
        ResourceLoader.Load<PackedScene>("res://Nodes/Scenes/Inventory/StorageFragmentScene.tscn");
    private Dictionary<FoldableContainer, float> _storageDictionary = [];

    public void SetDetails(Node<WearsClothingComponent> node)
    {
        _nodeSystemManager.InjectNodeSystemDependencies(this);
        // Get rid of preview template
        foreach (var child in GetChildren())
            child.QueueFree();
        
        // All foldable containers will belong to one foldable group,
        // so that opening one storage collapses the other ones
        var foldableGroup = new FoldableGroup();
        foldableGroup.AllowFoldingAll = true;
        
        // For each item equipped that has storage, add it to the inventory scene
        foreach (var (_, clothingNode) in node.Comp.ClothingSlots)
        {
            if (clothingNode == null)
                continue;
            
            if (!_nodeManager.TryGetComponent<StorageComponent>(clothingNode, out var storageComponent))
                continue;
            
            CreateStorageListing((clothingNode, storageComponent), foldableGroup);
        }
    }

    private void CreateStorageListing(Node<StorageComponent> node, FoldableGroup foldableGroup)
    {
        var foldableContainer = new FoldableContainer();
        foldableContainer.FoldableGroup = foldableGroup;
        foldableContainer.SetTitleTextOverrunBehavior(TextServer.OverrunBehavior.TrimEllipsis);
        if (_descriptionSystem.TryGetDisplayName(node, out var name))
            foldableContainer.SetTitle(name);
        AddChild(foldableContainer);
        MoveChild(foldableContainer, 0); // Add to the top of the list, not the bottom.

        var vBoxContainer = new VBoxContainer();
        foldableContainer.AddChild(vBoxContainer);

        var storageSceneSystem = _storageFragmentScene.Instantiate<StorageSceneSystem>();
        foldableContainer.AddChild(storageSceneSystem);
        storageSceneSystem.SetDetails(node);
    }
}