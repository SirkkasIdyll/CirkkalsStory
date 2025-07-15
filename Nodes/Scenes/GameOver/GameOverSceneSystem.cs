using CS.Nodes.UI.Audio;
using CS.Nodes.UI.ButtonTypes;
using CS.SlimeFactory;
using Godot;

namespace CS.Nodes.Scenes.GameOver;

public partial class GameOverSceneSystem : Control
{
	private readonly NodeManager _nodeManager = NodeManager.Instance;
	
	[ExportCategory("Instantiated")]
	[Export] private AudioStream? _bgm;
	[Export] private PackedScene? _nextScene;
	
	[ExportCategory("Owned")]
	[Export] private StandardButton _acceptButton = default!;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var bgmPlayer = GetNode<LoopingAudioStreamPlayer2DSystem>("/root/MainScene/BGMAudioStreamPlayer2D");
		if (bgmPlayer != null)
		{
			bgmPlayer.SetStream(_bgm);
			bgmPlayer.Play();
		}
		
		_acceptButton.Pressed += OnAcceptButtonPressed;
		_acceptButton.GrabFocus();
	}

	private void OnAcceptButtonPressed()
	{
		if (_nextScene != null)
		{
			var signal = new ChangeActiveSceneSignal(_nextScene);
			_nodeManager.SignalBus.EmitChangeActiveSceneSignal(this, ref signal);
		}
	}
}