using CS.Nodes.UI.ButtonTypes;
using CS.Nodes.Scenes.Options;
using Godot;

namespace CS.Nodes.Scenes.EscapeMenu;

public partial class EscapeMenuSceneSystem : Control
{
	[ExportCategory("Owned")]
	[Export] private AudioStreamPlayer2D _cancelSound = null!;
	[Export] private StandardButton _continue = null!;
	[Export] private StandardButton _options = null!;
	[Export] private EscapeButton _quit = null!;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_continue.Pressed += OnContinueButtonPressed;
		_continue.EscapePressed += OnEscapePressed;
		_options.Pressed += OnOptionsButtonPressed;
		_options.EscapePressed += OnEscapePressed;
		_quit.Pressed += OnQuitButtonPressed;
		
		VisibilityChanged += OnVisibilityChanged;
		_continue.GrabFocus();
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
			
			_continue.GrabFocus();
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
		_quit.GrabFocus();
		_cancelSound.Play();
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
	
	private void OnVisibilityChanged()
	{
		if (Visible)
			_continue.GrabFocus();
	}
}