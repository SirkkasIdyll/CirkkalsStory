using CS.Scripts.UI.OptionUI;
using Godot;

namespace CS.Scripts.UI.StartMenuUi;

public partial class StartMenu : AspectRatioContainer
{
	[Export] private LoopingAudioStreamPlayer2D _startMenuMusic;
	[Export] private StartMenuItemList _startMenuItemList;
	[Export] private Options _options;

	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_startMenuMusic.Playing = true;
		_startMenuItemList.StartGameSelected += OnStartGameSelected;
		_startMenuItemList.OptionsSelected += OnOptionsSelected;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}

	private void OnStartGameSelected()
	{
		var battle = GD.Load<PackedScene>("res://Scenes/Battle/battle_scene.tscn");
		var uiLayer = GetNode<CanvasLayer>("/root/MainGameScene/CanvasLayerForUI");
		uiLayer.AddChild(battle.Instantiate());
		Visible = false;
		_startMenuMusic.Playing = false;
	}

	private void OnOptionsSelected()
	{
		Visible = false;
		_options.Visible = true;
	}
}