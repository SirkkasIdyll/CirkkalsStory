using CS.Components.Clothing;
using CS.Components.Inventory;
using CS.SlimeFactory;
using Godot;

namespace CS.Components.Appearance;

public partial class AppearanceSystem : NodeSystem
{
    public override void _Ready()
    {
        base._Ready();
        
        _nodeManager.SignalBus.ClothingEquippedSignal += OnClothingEquipped;
        _nodeManager.SignalBus.ItemPutInHandSignal += OnItemPutInHand;
        _nodeManager.SignalBus.ItemPutInStorageSignal += OnItemPutInStorage;

    }

    private void OnClothingEquipped(Node<WearsClothingComponent> node, ref ClothingEquippedSignal args)
    {
        if (node.Owner is CharacterBody2D characterBody2D)
            OrientClothingToBody(characterBody2D);
        
        AttachItemInvisibly(node, args.Clothing);
    }

    private void OnItemPutInHand(Node<WearsClothingComponent> node, ref ItemPutInHandSignal args)
    {
        AttachItemInvisibly(node, args.Storable);
    }

    private void OnItemPutInStorage(Node<StorageComponent> node, ref ItemPutInStorageSignal args)
    {
        AttachItemInvisibly(node, args.Storable);
    }

    /// <summary>
    /// Attaches a node to another node so that it follows it around invisibly.
    /// </summary>
    public void AttachItemInvisibly(Node main, Node nodeToAttach)
    {
        if (main is not Node2D mainNode2D || nodeToAttach is not Node2D node2DToAttach)
            return;
        
        node2DToAttach.SetVisible(false);
        nodeToAttach.Reparent(main, false);
        node2DToAttach.GlobalPosition = mainNode2D.GlobalPosition;
    }

    /// <summary>
    /// Orients the character's sprite to the correct facing given the inputs
    /// </summary>
    public void OrientCharacterSprite(CharacterBody2D node, bool? faceRight = null, bool? faceForward = null)
    {
        var canvasGroup = node.GetNode<CanvasGroup>("CanvasGroup");

        // Swaps between the front-facing and back-facing sprites
        if (faceForward != null)
        {
            foreach (var child in canvasGroup.GetChildren())
            {
                if (child is not AnimatedSprite2D animatedSprite2D)
                    continue;

                if (animatedSprite2D.SpriteFrames == null)
                    continue;
            
                animatedSprite2D.Animation = faceForward.Value ? "default" : "back";
            }
        }

        
        // Turns the character left or right by flipping the x scale
        if (faceRight != null)
        {
            var orientationVector = faceRight.Value ? new Vector2(1, 1) : new Vector2(-1, 1);
            var tween = CreateTween();
            tween.SetEase(Tween.EaseType.Out);
            tween.TweenProperty(canvasGroup, "scale", orientationVector, 0.1f);
        }
    }

    public void OrientClothingToBody(CharacterBody2D node)
    {
        var canvasGroup = node.GetNode<CanvasGroup>("CanvasGroup");

        var body = canvasGroup.GetChildOrNull<AnimatedSprite2D>(0);
        if (body == null)
            return;

        var animation = body.Animation;
        var orientationVector = body.Scale;
        
        foreach (var child in canvasGroup.GetChildren())
        {
            if (child is not AnimatedSprite2D animatedSprite2D)
                continue;

            if (animatedSprite2D.SpriteFrames == null)
                continue;

            animatedSprite2D.Animation = animation;
            animatedSprite2D.Scale = orientationVector;
        }
    }
}