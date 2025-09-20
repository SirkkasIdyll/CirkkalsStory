using Godot;

namespace CS.Nodes.UI.CustomWindow;

public partial class CustomWindowSystem : Control
{
	private Vector2? _prevMousePositionWhileTitlePressed;

	[ExportCategory("Instantiated")]
	[Export] public Control? Content;
	
	[ExportCategory("Owned")]
	[Export] private Button _closeButton = null!;
	[Export] public Button Title = null!;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_closeButton.Pressed += QueueFree;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		DragTitleToRepositionWindow();
	}

	/// <summary>
	/// When the title is pressed, the position of the mouse is saved
	/// The window's position is adjusted based on the difference between the saved mouse position and the current mouse position
	/// Updated each frame
	/// </summary>
	private void DragTitleToRepositionWindow()
	{
		if (Title.IsPressed())
		{
			if (_prevMousePositionWhileTitlePressed == null)
				_prevMousePositionWhileTitlePressed = GetGlobalMousePosition();
			
			var diff = _prevMousePositionWhileTitlePressed - GetGlobalMousePosition();
			SetGlobalPosition((Vector2)(GetGlobalPosition() - diff));
			_prevMousePositionWhileTitlePressed = GetGlobalMousePosition();
			return;
		}
		
		_prevMousePositionWhileTitlePressed = null;
	}
}