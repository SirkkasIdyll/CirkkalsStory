using Godot;
using Godot.Collections;

namespace CS.Nodes.UI.Tooltip;

public partial class CustomTooltip : Control
{
	[ExportCategory("Owned")]
	[Export] private RichTextLabel _tooltipTitle = null!;
	[Export] private VBoxContainer _tooltipBulletpoints = null!;
	[Export] private RichTextLabel _tooltipDescription = null!;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// MouseExited += QueueFree;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		base._UnhandledInput(@event);
		
		if (@event is not InputEventMouseButton)
			return;
		
		if (@event.IsActionPressed("primary_interact") || @event.IsActionPressed("secondary_interact"))
			QueueFree();
	}

	public void SetTooltipTitle(string title)
	{
		_tooltipTitle.SetText("[b]" + title + "[/b]");
	}

	public void SetTooltipDescription(string description)
	{
		_tooltipDescription.SetText(description);
	}

	public void SetTooltipBulletpoints(Array<string> bulletpoints)
	{
		var children = _tooltipBulletpoints.GetChildren();
		foreach (var child in children)
			child.QueueFree();
		
		foreach (var bulletpoint in bulletpoints)
		{
			var richTextLabel = new RichTextLabel();
			richTextLabel.BbcodeEnabled = true;
			richTextLabel.SetFitContent(true);
			// richTextLabel.SetAutowrapMode(TextServer.AutowrapMode.Off);
			richTextLabel.SetText(bulletpoint);
			_tooltipBulletpoints.AddChild(richTextLabel);
		}
	}
}