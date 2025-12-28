using CS.Components.Clothing;
using CS.Components.Description;
using CS.Components.Inventory;
using CS.Components.Player;
using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Nodes.Scenes.Inventory;

/// <summary>
/// Displays all worn storage equipped to a mob.
/// </summary>
public partial class InventorySceneSystem : VBoxContainer, IModifiableScene
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
    private FoldableGroup _foldableGroup = new();
    private Dictionary<Node, FoldableContainer> _foldableDictionary = [];

    private Node? _uiOwner;
    private WearsClothingComponent? _uiWearsClothingComponent;

    public override void _ExitTree()
    {
        base._ExitTree();

        _nodeManager.SignalBus.ClothingEquippedSignal -= OnClothingEquipped;
        _nodeManager.SignalBus.ClothingUnequippedSignal -= OnClothingUnequipped;
    }

    public override void _Ready()
    {
        base._Ready();
        _nodeSystemManager.InjectNodeSystemDependencies(this);

        _nodeManager.SignalBus.ClothingEquippedSignal += OnClothingEquipped;
        _nodeManager.SignalBus.ClothingUnequippedSignal += OnClothingUnequipped;
    }

    private void OnClothingEquipped(Node<WearsClothingComponent> node, ref ClothingEquippedSignal args)
    {
        if (node.Owner != _uiOwner)
            return;

        if (!_nodeManager.TryGetComponent<StorageComponent>(args.Clothing, out var storageComponent))
            return;

        // Storage listing already exists, don't create a duplicate.
        if (_foldableDictionary.TryGetValue(args.Clothing, out _))
            return;

        CreateStorageListing((args.Clothing, storageComponent));
    }

    private void OnClothingUnequipped(Node<WearsClothingComponent> node, ref ClothingUnequippedSignal args)
    {
        if (node.Owner != _uiOwner)
            return;

        if (!_foldableDictionary.TryGetValue(args.Clothing, out var foldableContainer))
            return;

        RemoveChild(foldableContainer);
        _foldableDictionary.Remove(args.Clothing);
    }

    public void ModifyScene(Node node)
    {
        _uiOwner = node;

        if (!_nodeManager.TryGetComponent<WearsClothingComponent>(node, out var wearsClothingComponent))
            return;

        _uiWearsClothingComponent = wearsClothingComponent;

        // Foldable groups connect the storage UIs together so that only is expanded at a time
        _foldableGroup.AllowFoldingAll = true;

        // Get rid of preview template
        foreach (var child in GetChildren())
            child.QueueFree();

        // For each item equipped that has storage, add it to the inventory scene
        foreach (var (_, clothingNode) in _uiWearsClothingComponent.ClothingSlots)
        {
            if (clothingNode == null)
                continue;

            if (!_nodeManager.TryGetComponent<StorageComponent>(clothingNode, out var storageComponent))
                continue;

            CreateStorageListing((clothingNode, storageComponent));
        }
    }

    private void CreateStorageListing(Node<StorageComponent> node)
    {
        var foldableContainer = new FoldableContainer();
        foldableContainer.FoldableGroup = _foldableGroup;
        foldableContainer.SetTitleTextOverrunBehavior(TextServer.OverrunBehavior.TrimEllipsis);
        foldableContainer.MouseEntered += () =>
        {
            if (!GetViewport().GuiIsDragging())
                return;

            // Unfold the container if the user is attempting to drag something into it
            foldableContainer.SetFolded(false);
        };

        AddChild(foldableContainer);
        MoveChild(foldableContainer, 0); // Add to the top of the list, not the bottom.
        _foldableDictionary.Add(node, foldableContainer);

        if (_descriptionSystem.TryGetDisplayName(node, out var name))
            foldableContainer.SetTitle(name);

        var vBoxContainer = new VBoxContainer();
        foldableContainer.AddChild(vBoxContainer);

        var storageSceneSystem = _storageFragmentScene.Instantiate<StorageSceneSystem>();
        foldableContainer.AddChild(storageSceneSystem);
        storageSceneSystem.ModifyScene(node);
    }
}