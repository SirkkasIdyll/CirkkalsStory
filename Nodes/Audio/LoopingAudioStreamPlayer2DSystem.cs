using Godot;

namespace CS.Nodes.Audio;

public partial class LoopingAudioStreamPlayer2DSystem : AudioStreamPlayer2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Finished += OnFinished;
	}

	private void OnFinished()
	{
		Play();
	}
}