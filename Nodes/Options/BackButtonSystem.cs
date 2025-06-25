using Godot;

namespace CS.Nodes.Options;

public partial class BackButtonSystem : Button
{
	[Export] private AudioStreamPlayer2D? _confirmSound;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Pressed += OnPressed;
		MouseEntered += OnMouseEntered;
		MouseExited += OnMouseExited;
	}

	private void OnPressed()
	{
		_confirmSound?.Play();
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