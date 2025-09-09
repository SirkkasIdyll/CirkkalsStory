using CS.Components.Clothing;
using CS.SlimeFactory;
using Godot;

namespace CS.Components.Appearance;

public partial class AppearanceSystem : NodeSystem
{
    public override void _Ready()
    {
        base._Ready();

        _nodeManager.SignalBus.ClothingEquippedSignal += OnClothingEquipped;
    }

    private void OnClothingEquipped(Node<WearsClothingComponent> node, ref ClothingEquippedSignal args)
    {
        if (node.Owner is CharacterBody2D characterBody2D)
            OrientClothingToBody(characterBody2D);
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