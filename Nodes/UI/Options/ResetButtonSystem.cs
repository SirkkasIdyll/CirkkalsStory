using Godot;

namespace CS.Nodes.UI.Options;

public partial class ResetButtonSystem : Button
{
	[ExportCategory("Instantiated")]
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
		_cancelSound?.Play();
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