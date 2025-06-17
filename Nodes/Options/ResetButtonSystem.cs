using Godot;

namespace CS.Nodes.Options;

public partial class ResetButtonSystem : Button
{
	[Signal]
	public delegate void OptionsResetEventHandler();

	[Export] private AudioStreamPlayer2D? _cancelSound;
	
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
		
		if (_cancelSound != null)
			_cancelSound.Playing = true;
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