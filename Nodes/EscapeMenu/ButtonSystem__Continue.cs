using CS.Nodes.Audio;
using Godot;

namespace CS.Nodes.EscapeMenu;

public partial class ButtonSystem__Continue : Button
{
	[Export] private FluctuatingAudioStreamPlayer2DSystem? _selectSound;
	[Export] private AudioStreamPlayer2D? _confirmSound;
	
	[Signal]
	public delegate void EscapePressedEventHandler();
	
	// Called when the node enters the scene tree for the first time.
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