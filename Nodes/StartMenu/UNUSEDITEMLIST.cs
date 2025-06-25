using Godot;

namespace CS.Nodes.StartMenu;

public partial class UNUSEDITEMLIST : ItemList
{
	[Export] private AudioStreamPlayer2D? _selectSound;
	[Export] private AudioStreamPlayer2D? _confirmSound;
	
	[Signal]
	public delegate void StartGameSelectedEventHandler();

	[Signal]
	public delegate void OptionsSelectedEventHandler();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GrabFocus();
		ItemActivated += OnItemActivated;
		ItemClicked += OnItemClicked;
		ItemSelected += OnItemSelected;
		MouseEntered += OnMouseEntered;
		MouseExited += OnMouseExited;
		VisibilityChanged += OnVisibilityChanged;
	}

	private void change_to_color(Color color)
	{
		AddThemeColorOverride("font_selected_color", color);
	}
	
	private void OnItemActivated(long index)
	{
		if (_confirmSound != null)
			_confirmSound.Playing = true;

		if (index == 0)
			EmitSignal(SignalName.StartGameSelected);
		
		if (index == 2)
			EmitSignal(SignalName.OptionsSelected);
		
		if (index == 3)
			GetTree().Quit();
	}

	/// <summary>
	/// When an item is left-clicked, it gets immediately activated.
	/// </summary>
	/// <param name="index">Which item was selected</param>
	/// <param name="position">Where the user clicked</param>
	/// <param name="mouseButton">Which button the user clicked with</param>
	private void OnItemClicked(long index, Vector2 position, long mouseButton)
	{
		if (Input.IsMouseButtonPressed(MouseButton.Left))
			OnItemActivated(index);
	}

	private void OnItemSelected(long index)
	{
		// Prevents select sound from being played on top of the confirmation sound if user clicked with mouse button
		if (Input.IsMouseButtonPressed(MouseButton.Left))
			return;

		if (_selectSound != null)
			_selectSound.Playing = true;
		
		Tween tween = CreateTween();
		Callable callable = new Callable(this, MethodName.change_to_color);
		tween.TweenMethod(callable, new Color((float) 0.732, (float) 0.744, (float) 0.748), new Color(1, 1, 1), 0.15f);
	}
	
	private void OnMouseEntered()
	{
		SetDefaultCursorShape(CursorShape.PointingHand);
	}

	private void OnMouseExited()
	{
		SetDefaultCursorShape(CursorShape.Arrow);
	}	
	
	private void OnVisibilityChanged()
	{
		GrabFocus();
	}
	
	public override void _UnhandledInput(InputEvent @event)
	{
		var last = ItemCount - 1;
		
		// Close the game when the user hits escape
		if (@event.IsActionPressed("ui_cancel") && HasFocus())
			GetTree().Quit();
		
		// Loop from start to quit when user presses up
		if (@event.IsActionPressed("ui_up") && IsSelected(0))
		{
			Select(last);
			OnItemSelected(last);
			GetViewport().SetInputAsHandled();
		}
		
		// Loop from quit to start when user presses down
		if (@event.IsActionPressed("ui_down") && IsSelected(last))
		{
			Select(0);
			OnItemSelected(0);
			GetViewport().SetInputAsHandled();
		}
	}
}