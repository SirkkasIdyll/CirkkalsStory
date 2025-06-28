using Godot;

namespace CS.Nodes.Combat;

public partial class ButtonSystem__Target : Button
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MouseEntered += OnMouseEntered;
		MouseExited += OnMouseExited;
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