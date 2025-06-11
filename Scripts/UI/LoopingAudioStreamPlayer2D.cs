using Godot;

namespace CS.Scripts.UI;

public partial class LoopingAudioStreamPlayer2D : AudioStreamPlayer2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Finished += OnFinished;
	}

	private void OnFinished()
	{
		Playing = true;
	}
}