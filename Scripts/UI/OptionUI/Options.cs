using System.Collections.Generic;
using CS.Scripts.UI.StartMenuUi;
using Godot;

namespace CS.Scripts.UI.OptionUI;

public partial class Options : AspectRatioContainer
{
	[Export] private StartMenu _startMenu;
	[Export] private AudioStreamPlayer2D _cancelSound;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel") && Visible)
		{
			_startMenu.Visible = true;
			Visible = false;
			_cancelSound.Playing = true;
			GetViewport().SetInputAsHandled();
		}
	}
}