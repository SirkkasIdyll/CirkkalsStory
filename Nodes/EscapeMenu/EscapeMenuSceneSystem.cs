using CS.Nodes.Options;
using Godot;

namespace CS.Nodes.EscapeMenu;

public partial class EscapeMenuSceneSystem : Control
{
	[ExportCategory("Owned")]
	[Export] private AudioStreamPlayer2D? _cancelSound;
	[Export] private ButtonSystem__Continue? _continue;
	[Export] private ButtonSystem__Options? _options;
	[Export] private ButtonSystem__Quit? _quit;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (_cancelSound == null ||
		    _continue == null ||
		    _options == null ||
		    _quit == null)
			GD.PrintErr("Owned property is null\n" + System.Environment.StackTrace);
		
		if (_continue != null)
		{
			_continue.Pressed += OnContinueButtonPressed;
			_continue.EscapePressed += OnEscapePressed;
			_continue.GrabFocus();
		}

		if (_options != null)
		{
			_options.Pressed += OnOptionsButtonPressed;
			_options.EscapePressed += OnEscapePressed;
		}
		
		if (_quit != null)
			_quit.Pressed += OnQuitButtonPressed;
	}
	
	/// <summary>
	/// If nothing is in focus, try to grab focus on the continue or quit button
	/// </summary>
	/// <param name="event"></param>
	public override void _UnhandledInput(InputEvent @event)
	{
		if (!IsVisibleInTree())
			return;
		
		if (@event is not InputEventKey eventKey)
			return;
		
		if (GetViewport().GuiGetFocusOwner() == null)
		{
			if (eventKey.IsPressed() && eventKey.Keycode == Key.Escape)
			{
				OnEscapePressed();
				GetViewport().SetInputAsHandled();
				return;
			}
			
			_continue?.GrabFocus();
			GetViewport().SetInputAsHandled();
		}
	}

	/// <summary>
	/// Gets rid of the escape menu
	/// </summary>
	private void OnContinueButtonPressed()
	{
		QueueFree();
	}

	/// <summary>
	/// Focuses on the quit button when ESC is pressed
	/// </summary>
	private void OnEscapePressed()
	{
		_quit?.GrabFocus();
		_cancelSound?.Play();
	}

	/// <summary>
	/// Makes the options scene the most visible child node and hides the escape menu
	/// </summary>
	private void OnOptionsButtonPressed()
	{
		SetVisible(false);
		
		var optionsScene = GetNode<OptionsSceneSystem>("/root/MainScene/CanvasLayer/Options");
		optionsScene.GetParent().MoveChild(optionsScene, optionsScene.GetParent().GetChildCount());
		optionsScene.SetVisible(true);
		optionsScene.PreviousScene = this;
	}

	/// <summary>
	/// Closes the game
	/// </summary>
	private void OnQuitButtonPressed()
	{
		GetTree().Quit();
	}
}