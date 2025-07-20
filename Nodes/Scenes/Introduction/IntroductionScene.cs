using CS.Nodes.UI.Audio;
using CS.Nodes.UI.ButtonTypes;
using CS.SlimeFactory;
using Godot;

namespace CS.Nodes.Scenes.Introduction;

public partial class IntroductionScene : Control
{
	private readonly NodeManager _nodeManager = NodeManager.Instance;
	
	[ExportCategory("Instantiated")]
	[Export] private AudioStream? _bgm;
	[Export] private PackedScene? _nextScene;
	[Export] private PackedScene? _mobPlayer;
	
	[ExportCategory("Owned")]
	[Export] private StandardButton _acceptButton = null!;
	
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