using Godot;

namespace CS.Scripts.UI.OptionUI;

public partial class ResetButton : Button
{
	[Signal]
	public delegate void OptionsResetEventHandler();

	[Export] private AudioStreamPlayer2D CancelSound;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Pressed += OnPressed;
		MouseEntered += OnMouseEntered;
		MouseExited += OnMouseExited;
	}

	private void OnPressed()
	{
		EmitSignal(SignalName.OptionsReset);
		CancelSound.Playing = true;
	}
	
	private void OnMouseEntered()
	{
		SetDefaultCursorShape(CursorShape.PointingHand);
	}

	private void OnMouseExited()
	{
		SetDefaultCursorShape(CursorShape.Arrow);
	}	
}