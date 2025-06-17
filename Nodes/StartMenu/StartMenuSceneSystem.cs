using CS.Nodes.Audio;
using CS.Nodes.Options;
using Godot;

namespace CS.Nodes.StartMenu;

public partial class StartMenuSceneSystem : AspectRatioContainer
{
	[Export] private LoopingAudioStreamPlayer2DSystem? _loopingStartMenuMusic;
	[Export] private StartMenuItemListSystem? _startMenuItemList;
	[Export] private OptionsSceneSystem? _options;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (_loopingStartMenuMusic != null)
			_loopingStartMenuMusic.Playing = true;

		if (_startMenuItemList != null)
		{
			_startMenuItemList.StartGameSelected += OnStartGameSelected;
			_startMenuItemList.OptionsSelected += OnOptionsSelected;
		}
	}
	
	private void OnStartGameSelected()
	{
		var battle = GD.Load<PackedScene>("res://Nodes/Battle/CombatScene.tscn");
		var uiLayer = GetNode<CanvasLayer>("/root/MainScene/CanvasLayer");
		uiLayer.AddChild(battle.Instantiate());
		Visible = false;

		if (_loopingStartMenuMusic != null)
			_loopingStartMenuMusic.Playing = false;
	}

	private void OnOptionsSelected()
	{
		Visible = false;
		
		if (_options != null)
			_options.Visible = true;
	}
}