using CS.Components.Player;
using CS.SlimeFactory;
using CS.SlimeFactory.Signals;
using Godot;

namespace CS.Components.CameraAim;

public partial class CameraAimSystem : NodeSystem
{
    [InjectDependency] private readonly PlayerManagerSystem _playerManagerSystem = null!;
    
    private const float CameraAimSpeed = 6f;
    private const float CameraResetSpeed = 10.0f;
    private const float Tiles = 1f;
    private const double HoldFor = 0.3;
    private double _timeHeld = 0;
    
    public override void _Process(double delta)
    {
        base._Process(delta);
        
        var node = _playerManagerSystem.TryGetPlayer();
        if (node is not CharacterBody2D characterBody2D)
            return;
        
        var camera = GetViewport().GetCamera2D();
        if (camera == null)
            return;
        
        // Not aiming
        if (!Input.IsActionPressed("aim"))
        {
            _timeHeld = 0;
            OnNotAiming(characterBody2D, camera, delta);
            return;
        }
        
        // Aiming, but not for long enough
        _timeHeld += delta;
        if (_timeHeld < HoldFor)
            return;

        var signal = new CurrentlyAimingSignal(camera, delta);
        _nodeManager.SignalBus.EmitCurrentlyAimingSignal(characterBody2D, ref signal);
        OnCurrentlyAiming(characterBody2D, ref signal);
    }

    private void OnCurrentlyAiming(CharacterBody2D characterBody2D, ref CurrentlyAimingSignal args)
    {
        var cameraScene = args.Camera2D.GetParent<Node2D>();
        // Given that the viewport size is the full width/height of the window
        // And the mouse offset is half that
        // We're only going as far as like 1/2 of the possibleDistance stated
        var cameraOffset = cameraScene.GlobalPosition - args.Camera2D.GlobalPosition;
        var mouseOffset = (GetGlobalMousePosition() - cameraOffset - cameraScene.GetGlobalPosition());
        var viewportSize = GetViewportRect().Size / args.Camera2D.Scale;
        var possibleDistance = Tiles * 32;
        var newCameraPosition = new Vector2(
            cameraScene.GetGlobalPosition().X + mouseOffset.X / viewportSize.X * possibleDistance,
            cameraScene.GetGlobalPosition().Y + mouseOffset.Y / viewportSize.Y * possibleDistance);
        float weight = 1f - Mathf.Exp(-CameraAimSpeed * (float)args.Delta); 
        args.Camera2D.SetGlobalPosition(args.Camera2D.GlobalPosition.Lerp(newCameraPosition, weight));
    }

    // Move camera back towards default position
    private void OnNotAiming(CharacterBody2D characterBody2D, Camera2D camera2D, double delta)
    {
        var cameraScene = camera2D.GetParent<Node2D>();
        if (cameraScene.GlobalPosition == camera2D.GlobalPosition)
            return;
        
        float weight = 1f - Mathf.Exp(-CameraResetSpeed * (float)delta);
        camera2D.SetGlobalPosition(camera2D.GlobalPosition.Lerp(cameraScene.GetGlobalPosition(), weight));
    }
}

public partial class CurrentlyAimingSignal : UserSignalArgs
{
    public Camera2D Camera2D;
    public double Delta;

    public CurrentlyAimingSignal(Camera2D camera2D, double delta)
    {
        Camera2D = camera2D;
        Delta = delta;
    }
}