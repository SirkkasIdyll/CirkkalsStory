using CS.SlimeFactory;
using Godot;

namespace CS.Components.CameraAim;

public partial class CameraAimSystem : NodeSystem
{
    private const float CameraAimSpeed = 6f;
    private const float CameraResetSpeed = 10.0f;
    private const float Tiles = 1f;
    private const double HoldFor = 0.3;
    private double _timeHeld = 0;
    
    public override void _Process(double delta)
    {
        base._Process(delta);

        float weight;
        // No camera
        var camera = GetViewport().GetCamera2D();
        if (camera == null)
            return;
        
        var parent = camera.GetParentOrNull<Node2D>();
        if (parent == null)
            return;
        
        // Not aiming
        if (!Input.IsActionPressed("aim"))
        {
            _timeHeld = 0;
            weight = 1f - Mathf.Exp(-CameraResetSpeed * (float)delta);
            camera.SetGlobalPosition(camera.GlobalPosition.Lerp(parent.GetGlobalPosition(), weight));
            return;
        }
        
        // Aiming, but not for long enough
        _timeHeld += delta;
        if (_timeHeld < HoldFor)
            return;
        
        // Now aiming the camera
        // Given that the viewport size is the full width/height of the window
        // And the mouse offset is half that
        // We're only going as far as like 1/2 of the possibleDistance stated
        var cameraOffset = parent.GlobalPosition - camera.GlobalPosition;
        var mouseOffset = (GetGlobalMousePosition() - cameraOffset - parent.GetGlobalPosition());
        var viewportSize = GetViewportRect().Size / camera.Scale;
        var possibleDistance = Tiles * 32;
        var newCameraPosition = new Vector2(
            parent.GetGlobalPosition().X + mouseOffset.X / viewportSize.X * possibleDistance,
            parent.GetGlobalPosition().Y + mouseOffset.Y / viewportSize.Y * possibleDistance);
        weight = 1f - Mathf.Exp(-CameraAimSpeed * (float)delta); 
        camera.SetGlobalPosition(camera.GlobalPosition.Lerp(newCameraPosition, weight));
    }
}