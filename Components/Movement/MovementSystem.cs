using CS.Components.Appearance;
using CS.Components.Player;
using CS.SlimeFactory;
using CS.SlimeFactory.Signals;
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

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (!@event.IsActionPressed("primary_interact"))
            return;
        
        var node = _playerManagerSystem.TryGetPlayer();

        if (node is not CharacterBody2D characterBody)
            return;
            
        var facingRight = characterBody.GlobalPosition.X < GetGlobalMousePosition().X;
        var facingForward = characterBody.GlobalPosition.Y - 50 < GetGlobalMousePosition().Y;
        _appearanceSystem.OrientCharacterSprite(characterBody, facingRight, facingForward);
    }

    public void MovePlayer()
    {
        var node = _playerManagerSystem.TryGetPlayer();
        var inputDirection = Input.GetVector("left", "right", "up", "down");

        if (node is not CharacterBody2D characterBody)
            return;
        
        if (!_nodeManager.TryGetComponent<MovementComponent>(node, out var movementComponent))
            return;

        if (inputDirection == Vector2.Zero)
        {
            movementComponent.SoundEffect?.SetStreamPaused(true);
            return;
        }
        
        // Check if anything is preventing us from moving
        var movementAttemptSignal = new MovementAttemptSignal(characterBody);
        _nodeManager.SignalBus.EmitMovementAttemptSignal((node, movementComponent), ref movementAttemptSignal);
        
        if (movementAttemptSignal.Canceled)
            return;
        
        // Set desired movement speed before other systems modify it
        if (Input.IsActionPressed("shift_modifier"))
            characterBody.Velocity = inputDirection * movementComponent.WalkingSpeed;
        else
            characterBody.Velocity = inputDirection * movementComponent.RunningSpeed;

        // Emit movement signal so systems can modify velocity before moving, or update the sprite based on movement
        var movementSignal = new MovementSignal(characterBody);
        _nodeManager.SignalBus.EmitMovementSignal((node, movementComponent), ref movementSignal);
        
        characterBody.MoveAndSlide();
        
        // TODO: (move this outside of the movement system so that sounds can play based on tile walked on)
        // Play footstep sfx
        if (movementComponent.SoundEffect != null)
        {
            if (inputDirection != Vector2.Zero && !movementComponent.SoundEffect.Playing)
                if (movementComponent.SoundEffect.StreamPaused)
                    movementComponent.SoundEffect.SetStreamPaused(false); 
                else movementComponent.SoundEffect.Play();
        
            /*if (inputDirection == Vector2.Zero && movementComponent.SoundEffect.Playing)
                movementComponent.SoundEffect.SetStreamPaused(true);*/
        }
    }
}

public partial class MovementAttemptSignal : CancellableSignalArgs
{
    public CharacterBody2D CharacterBody2D;

    public MovementAttemptSignal(CharacterBody2D characterBody2D)
    {
        CharacterBody2D = characterBody2D;
    }
}

public partial class MovementSignal : UserSignalArgs
{
    public CharacterBody2D CharacterBody2D;

    public MovementSignal(CharacterBody2D characterBody2D)
    {
        CharacterBody2D = characterBody2D;
    }
}