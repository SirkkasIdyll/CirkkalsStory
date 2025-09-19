using CS.Components.Clothing;
using CS.Components.Description;
using CS.Components.Inventory;
using CS.Components.Player;
using CS.SlimeFactory;
using Godot;

namespace CS.Nodes.Scenes.Inventory;

public partial class ClothingTextureRect : TextureRect
{
    public Node? Item;
    public ClothingSlot? Slot;
    
    private readonly NodeManager _nodeManager = NodeManager.Instance;
    private readonly NodeSystemManager _nodeSystemManager = NodeSystemManager.Instance;
    [InjectDependency] private readonly ClothingSystem _clothingSystem = null!;
    [InjectDependency] private readonly DescriptionSystem _descriptionSystem = null!;
    [InjectDependency] private readonly PlayerManagerSystem _playerManagerSystem = null!;

    public override void _Ready()
    {
        base._Ready();
        
        _nodeSystemManager.InjectNodeSystemDependencies(this);
    }

    /// <summary>
    /// Just check if we get any node at all while the slot is unoccupied
    /// </summary>
    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        // Something is horribly wrong
        if (Slot == null)
            return false;

        if ((Node?)data == null)
            return false;

        return true;
    }

    /// <summary>
    /// Equip the clothing item to that slot, or swap the clothing item with the already equipped one
    /// </summary>
    public override void _DropData(Vector2 atPosition, Variant data)
    {
        var node = (Node)data;

        var player = _playerManagerSystem.TryGetPlayer();
        if (player == null)
            return;

        if (!_nodeManager.TryGetComponent<WearsClothingComponent>(player, out var wearsClothingComponent))
            return;

        if (Slot == ClothingSlot.Inhand &&
            _nodeManager.TryGetComponent<StorableComponent>(node, out var storableComponent))
        {
            _clothingSystem.TryPutItemInHand((player, wearsClothingComponent), (node, storableComponent));
            return;
        }

        if (_nodeManager.TryGetComponent<ClothingComponent>(node, out var clothingComponent))
        {
            _clothingSystem.TryEquipClothing((player, wearsClothingComponent), (node, clothingComponent), true);
            return;
        }
    }

    /// <summary>
    /// Returns the clothing item when dragged, if there is one
    /// </summary>
    /// <param name="atPosition"></param>
    /// <returns></returns>
    public override Variant _GetDragData(Vector2 atPosition)
    {
        if (Item != null)
        {
            var control = new Control();
            control.Scale = new Vector2(2, 2);
            
            var textureRect = (TextureRect)Duplicate();
            textureRect.SetModulate(new Color(1f, 1f, 1f, 0.4f));
            control.AddChild(textureRect);
            
            SetDragPreview(control);
        }
        
        return Item == null ? default(Variant) : Item;
    }
}