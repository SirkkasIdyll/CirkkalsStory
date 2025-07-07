using CS.Nodes.UI.Audio;
using CS.Nodes.UI.ButtonTypes;
using CS.Nodes.Scenes.Options;
using CS.SlimeFactory;
using Godot;

namespace CS.Nodes.Scenes.StartMenu;

public partial class StartMenuSceneSystem : Control
{
	private readonly NodeManager _nodeManager = NodeManager.Instance;
	
	[ExportCategory("Instantiated")]
	[Export] private LoopingAudioStreamPlayer2DSystem? _bgmPlayer;
	[Export] private PackedScene? _newGameScene;
	[Export] private PackedScene? _continueScene;
	[Export] private PackedScene? _achievementsScene;
	
	[ExportCategory("Owned")]
	[Export] private AudioStream _bgm = default!;
	[Export] private AudioStreamPlayer2D _cancelSound = default!;
	[Export] private StandardButton _newGame = default!;
	[Export] private StandardButton _continue = default!;
	[Export] private StandardButton _options = default!;
	[Export] private StandardButton _achievements = default!;
	[Export] private EscapeButton _quit = default!;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (_bgmPlayer != null)
		{
			_bgmPlayer.SetStream(_bgm);
			_bgmPlayer.Play();
		}

		_newGameScene = ResourceLoader.Load<PackedScene>("res://Nodes/Scenes/Introduction/IntroductionScene.tscn");

		_newGame.Pressed += OnNewGameButtonPressed;
		_newGame.EscapePressed += OnEscapePressed;
		_continue.Pressed += OnContinueButtonPressed;
		_continue.EscapePressed += OnEscapePressed;
		_options.Pressed += OnOptionsButtonPressed;
		_options.EscapePressed += OnEscapePressed;
		_achievements.Pressed += OnAchievementsButtonPressed;
		_achievements.EscapePressed += OnEscapePressed;
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
				OnEscapePressed();
				GetViewport().SetInputAsHandled();
				return;
			}
			
			_newGame.GrabFocus();
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
		_quit.GrabFocus();
		_cancelSound.Play();
	}
	
	/// <summary>
	/// When a new game is started, gets rid of the start menu scene and instantiates the designated scene
	/// </summary>
	private void OnNewGameButtonPressed()
	{
		if (_newGameScene == null)
			return;

		var duration = 0.2f;
		var tween = CreateTween().BindNode(this);
		tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 0), duration);
		tween.TweenCallback(Callable.From(QueueFree));

		var signal = new ChangeActiveSceneSignal(_newGameScene);
		_nodeManager.SignalBus.EmitChangeActiveSceneSignal(this, ref signal);
	}
	
	/// <summary>
	/// Hides the start menu and shows the options menu
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