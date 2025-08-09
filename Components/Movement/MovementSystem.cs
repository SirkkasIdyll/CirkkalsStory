using CS.Components.Player;
using CS.SlimeFactory;
using Godot;

namespace CS.Components.Movement;

public partial class MovementSystem : NodeSystem
{
    [InjectDependency] private readonly PlayerManagerSystem _playerManagerSystem = null!;
    
    public override void _Ready()
    {
        base._Ready();
    }

    // TODO: This shit is evil, it's hardcoded only to move the player character
    // Will not work when the world is inhabited with multiple PCs
    
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        
        MovePlayer();
    }

    public void MovePlayer()
    {
        var node = _playerManagerSystem.GetPlayer();

        if (node is not CharacterBody2D characterBody)
            return;
        
        if (!_nodeManager.TryGetComponent<MovementComponent>(node, out var movementComponent))
            return;
        
        var inputDirection = Input.GetVector("left", "right", "up", "down");
        
        if (Input.IsActionPressed("shift_modifier"))
            characterBody.Velocity = inputDirection * movementComponent.WalkingSpeed;
        else
            characterBody.Velocity = inputDirection * movementComponent.RunningSpeed;

        var sprite = characterBody.GetNode<AnimatedSprite2D>("SpriteGroup/AnimatedSprite2D");
        
        if (sprite != null)
        {
            if (inputDirection == Vector2.Left)
                sprite.FlipH = true;
            else if (inputDirection == Vector2.Right)
                sprite.FlipH = false;

            if (inputDirection == Vector2.Up)
                sprite.Animation = "back";
            else if (inputDirection == Vector2.Down)
                sprite.Animation = "default";

            if (float.Abs(characterBody.GlobalPosition.X - GetGlobalMousePosition().X) <= 250 &&
                float.Abs(characterBody.GlobalPosition.Y - GetGlobalMousePosition().Y) <= 250)
            {
                if (characterBody.GlobalPosition.X > GetGlobalMousePosition().X)
                    sprite.FlipH = true;
                else
                    sprite.FlipH = false;
                
                if (characterBody.GlobalPosition.Y - 50 > GetGlobalMousePosition().Y)
                    sprite.Animation = "back";
                else
                    sprite.Animation = "default";
            }
        }


        if (movementComponent.SoundEffect != null)
        {
            if (inputDirection != Vector2.Zero && !movementComponent.SoundEffect.Playing)
                if (movementComponent.SoundEffect.StreamPaused)
                    movementComponent.SoundEffect.SetStreamPaused(false); 
                else movementComponent.SoundEffect.Play();
        
            if (inputDirection == Vector2.Zero && movementComponent.SoundEffect.Playing)
                movementComponent.SoundEffect.SetStreamPaused(true);
        }

        characterBody.MoveAndSlide();
    }
}