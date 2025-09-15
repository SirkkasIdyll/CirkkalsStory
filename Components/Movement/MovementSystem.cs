using CS.Components.Appearance;
using CS.Components.Player;
using CS.SlimeFactory;
using Godot;

namespace CS.Components.Movement;

public partial class MovementSystem : NodeSystem
{
    [InjectDependency] private readonly AppearanceSystem _appearanceSystem = null!;
    [InjectDependency] private readonly PlayerManagerSystem _playerManagerSystem = null!;

    // TODO: This shit is evil, it's hardcoded only to move the player character
    // Will not work when the world is inhabited with multiple PCs
    
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        
        MovePlayer();
    }

    public void MovePlayer()
    {
        var node = _playerManagerSystem.TryGetPlayer();

        if (node is not CharacterBody2D characterBody)
            return;
        
        if (!_nodeManager.TryGetComponent<MovementComponent>(node, out var movementComponent))
            return;
        
        // Walk speed
        var inputDirection = Input.GetVector("left", "right", "up", "down");
        if (Input.IsActionPressed("shift_modifier"))
            characterBody.Velocity = inputDirection * movementComponent.WalkingSpeed;
        else
            characterBody.Velocity = inputDirection * movementComponent.RunningSpeed;
        
        // Orient sprite by mouse or by keyboard movement
        if (Input.IsActionPressed("aim"))
        {
            var facingRight = characterBody.GlobalPosition.X < GetGlobalMousePosition().X;
            var facingForward = characterBody.GlobalPosition.Y - 50 < GetGlobalMousePosition().Y;
            _appearanceSystem.OrientCharacterSprite(characterBody, facingRight, facingForward);
        }
        else
        {
            if (inputDirection.X < 0)
                _appearanceSystem.OrientCharacterSprite(characterBody, faceRight: false);
            else if (inputDirection.X > 0)
                _appearanceSystem.OrientCharacterSprite(characterBody, faceRight: true);
            
            if (inputDirection.Y < 0)
                _appearanceSystem.OrientCharacterSprite(characterBody, faceForward: false);
            else if (inputDirection.Y > 0)
                _appearanceSystem.OrientCharacterSprite(characterBody, faceForward: true);
        }
        
        // Play footstep sfx
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