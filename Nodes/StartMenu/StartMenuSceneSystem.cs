using CS.Nodes.Audio;
using CS.Nodes.Options;
using Godot;

namespace CS.Nodes.StartMenu;

public partial class StartMenuSceneSystem : AspectRatioContainer
{
	[Export] private LoopingAudioStreamPlayer2DSystem _loopingStartMenuMusic;
	[Export] private StartMenuItemListSystem _startMenuItemList;
	[Export] private OptionsSceneSystem _options;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_loopingStartMenuMusic.Playing = true;
		_startMenuItemList.StartGameSelected += OnStartGameSelected;
		_startMenuItemList.OptionsSelected += OnOptionsSelected;
	}
	
	private void OnStartGameSelected()
	{
		var battle = GD.Load<PackedScene>("res://Scenes/Battle/battle_scene.tscn");
		var uiLayer = GetNode<CanvasLayer>("/root/MainGameScene/CanvasLayerForUI");
		uiLayer.AddChild(battle.Instantiate());
		Visible = false;
		_loopingStartMenuMusic.Playing = false;
	}

	private void OnOptionsSelected()
	{
		Visible = false;
		_options.Visible = true;
	}
}