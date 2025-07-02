using CS.Nodes.UI.Audio;
using Godot;

namespace CS.Nodes.UI.StartMenu;

public partial class ButtonSystem__Continue : Button
{
    [ExportCategory("Instantiated")]
    [Export] private AudioStreamPlayer2D? _confirmSound;
    [Export] private FluctuatingAudioStreamPlayer2DSystem? _selectSound;
    
    [Signal]
    public delegate void EscapePressedEventHandler();

    public override void _Ready()
    {
        Pressed += OnButtonPressed;
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
    }
    
    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventKey eventKey && eventKey.IsPressed() && eventKey.Keycode == Key.Escape)
        {
            EmitSignalEscapePressed();
            GetViewport().SetInputAsHandled();
        }
    }

    private void OnButtonPressed()
    {
        _confirmSound?.Play();
    }
    
    private void OnMouseEntered()
    {
        SetDefaultCursorShape(CursorShape.PointingHand);
        _selectSound?.Play();
    }

    private void OnMouseExited()
    {
        SetDefaultCursorShape(CursorShape.Arrow);
    }	
}