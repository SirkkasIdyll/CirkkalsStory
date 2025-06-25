using CS.Nodes.Audio;
using CS.Nodes.Options;
using Godot;

namespace CS.Nodes.StartMenu;

public partial class StartMenuSceneSystem : Control
{
	[ExportCategory("Instantiated")]
	[Export] private LoopingAudioStreamPlayer2DSystem? _bgmPlayer;
	[Export] private PackedScene? _newGameScene;
	[Export] private PackedScene? _continueScene;
	[Export] private OptionsSceneSystem? _optionsScene;
	[Export] private PackedScene? _achievementsScene;
	
	[ExportCategory("Owned")]
	[Export] private AudioStream? _bgm;
	[Export] private ButtonSystem__NewGame? _newGame;
	[Export] private ButtonSystem__Continue? _continue;
	[Export] private ButtonSystem__Options? _options;
	[Export] private ButtonSystem__Achievements? _achievements;
	[Export] private ButtonSystem__Quit? _quit;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (_bgm != null && _bgmPlayer != null)
		{
			_bgmPlayer.SetStream(_bgm);
			_bgmPlayer.Play();
		}

		if (_newGame != null)
		{
			_newGame.Pressed += OnNewGameButtonPressed;
			_newGame.EscapePressed += OnEscapePressed;
		}

		if (_continue != null)
		{
			_continue.Pressed += OnContinueButtonPressed;
			_continue.EscapePressed += OnEscapePressed;
		}

		if (_options != null)
		{
			_options.Pressed += OnOptionsButtonPressed;
			_options.EscapePressed += OnEscapePressed;
		}

		if (_achievements != null)
		{
			_achievements.Pressed += OnAchievementsButtonPressed;
			_achievements.EscapePressed += OnEscapePressed;
		}
		
		if (_quit != null)
			_quit.Pressed += OnQuitButtonPressed;
	}
	
	/// <summary>
	/// If nothing is in focus, try to grab focus on the new game or quit buttons
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
				_quit?.GrabFocus();
				GetViewport().SetInputAsHandled();
				return;
			}
			
			_newGame?.GrabFocus();
			GetViewport().SetInputAsHandled();
		}
	}
	
	private void OnAchievementsButtonPressed()
	{
		
	}

	private void OnContinueButtonPressed()
	{
		
	}
	
	/// <summary>
	/// Focuses on the quit button when ESC is pressed
	/// </summary>
	private void OnEscapePressed()
	{
		_quit?.GrabFocus();
	}
	
	/// <summary>
	/// When a new game is started, gets rid of the start menu scene and instantiates the designated scene
	/// </summary>
	private void OnNewGameButtonPressed()
	{
		if (_newGameScene == null)
			return;
		
		var instantiatedScene = _newGameScene.Instantiate();
		GetParent().AddChild(instantiatedScene);
		QueueFree();
	}
	
	/// <summary>
	/// Hides the start menu and shows the options menu
	/// </summary>
	private void OnOptionsButtonPressed()
	{
		if (_optionsScene == null)
			return;
		
		SetVisible(false);
		_optionsScene.SetVisible(true);
		_optionsScene.PreviousScene = this;
	}
	
	/// <summary>
	/// Closes the game
	/// </summary>
	private void OnQuitButtonPressed()
	{
		GetTree().Quit();
	}
}