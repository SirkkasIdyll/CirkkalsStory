using Godot;

namespace CS;

public partial class MainSceneSystem : Node2D
{
    public Control? ActiveScene;
    
    [ExportCategory("Instantiated")]
    [Export] private PackedScene? _escapeMenuSceneSystem;
    
    [ExportCategory("Owned")]
    [Export] private CanvasLayer? _canvasLayer;

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventKey eventKey)
            return;
        
        if (_canvasLayer != null)
        {
            if (eventKey.IsPressed() && eventKey.Keycode == Key.Escape)
            {
                var node = _escapeMenuSceneSystem?.Instantiate();
                _canvasLayer?.AddChild(node);
                GetViewport().SetInputAsHandled();
            }
        }
    }
}