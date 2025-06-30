using CS.Components.StatusEffect;
using Godot;

namespace CS;

public partial class MainSceneSystem : Node2D
{
    public Control? ActiveScene;
    
    [ExportCategory("Instantiated")]
    [Export] private PackedScene? _escapeMenuSceneSystem;
    
    [ExportCategory("Owned")]
    [Export] private CanvasLayer? _canvasLayer;

    public override void _Ready()
    {
 
        // var system = new StatusEffectApplicatorSystem();
        // var system2 = new StatusEffectApplicatorSystem();
        // DirContents();
    }

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
    
    public void DirContents()
    {
        using var dir = DirAccess.Open("res://Components/StatusEffect");
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            while (fileName != "")
            {
                if (dir.CurrentIsDir())
                {
                    GD.Print($"Found directory: {fileName}");
                }
                else
                {
                    GD.Print($"Found file: {fileName}");
                }
                
                fileName = dir.GetNext();
            }
        }
        else
        {
            GD.Print("An error occurred when trying to access the path.");
        }
    }
}