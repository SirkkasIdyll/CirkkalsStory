using CS.Components.Description;
using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Nodes.Scenes.Inventory;

public partial class StorageItemButton : Button
{
    private readonly NodeManager _nodeManager = NodeManager.Instance;
    private readonly NodeSystemManager _nodeSystemManager = NodeSystemManager.Instance;
    [InjectDependency] private readonly DescriptionSystem _descriptionSystem = null!;

    public Node Item;

    public StorageItemButton(Node item)
    {
        Item = item;
    }

    public override void _Ready()
    {
        base._Ready();
        _nodeSystemManager.InjectNodeSystemDependencies(this);
        
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
    }
    
    /// <summary>
    /// Returns the clothing item when dragged, if there is one
    /// </summary>
    /// <param name="atPosition"></param>
    /// <returns></returns>
    public override Variant _GetDragData(Vector2 atPosition)
    {
        if (_descriptionSystem.TryGetSprite(Item, out var sprite))
        {
            var control = new Control();
            control.Scale = new Vector2(2, 2);

            var textureRect = new TextureRect();
            textureRect.Texture = sprite.Texture;
            textureRect.SetModulate(new Color(1f, 1f, 1f, 0.6f));
            control.AddChild(textureRect);
            
            SetDragPreview(control);
        }
        
        return Item;
    }
    
    private void OnMouseEntered()
    {
        SetDefaultCursorShape(CursorShape.PointingHand);
    }
    
    private void OnMouseExited()
    {
        SetDefaultCursorShape(CursorShape.Arrow);
    }	
}