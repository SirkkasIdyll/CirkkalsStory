using Godot;
using Godot.Collections;

namespace CS.Nodes.UI.DialogBox;

public partial class DialogBox : Control
{
	private Tween? _tween;
	// Smaller number means faster
	private float _timeSpentPerCharacter = 0.04f;
	private float _timeWaitedBetweenLines = 0.30f;
	
	[ExportCategory("Owned")]
	[Export] public Label Title = null!;
	[Export] public RichTextLabel Dialog = null!;

	[Signal]
	public delegate void DialogFinishedEventHandler();

	public override void _Ready()
	{
		base._Ready();

		if (Dialog.Text.Length > 0)
		{
			ShowDialogBox();
			return;
		}
		else
		{
			EmitSignalDialogFinished();	
			QueueFree();
		}
	}

	public override void _GuiInput(InputEvent @event)
	{
		base._GuiInput(@event);

		if (@event.IsPressed() && _tween != null && _tween.IsValid())
		{
			_tween.Kill();
			Dialog.SetVisibleCharacters(-1);
			GetViewport().SetInputAsHandled();
			return;
		}

		if (@event.IsPressed())
		{
			EmitSignalDialogFinished();	
			GetViewport().SetInputAsHandled();
			QueueFree();
		}
	}

	public void SetDetails(string title, string dialog)
	{
		Title.SetText(title);
		Dialog.SetText(dialog);
	}

	public void SetDetails(string title, Array<string> dialog)
	{
		var text = "";
		foreach (var line in dialog)
		{
			text += line + "\n";
		}
		Title.SetText(title);
		Dialog.SetText(text);
	}

	private void ShowDialogBox()
	{
		SetVisible(true);
		SetFocusMode(FocusModeEnum.All);
		GrabFocus();

		var visibleCharacters = 0;
		var tween = CreateTween();
		_tween = tween;
		for (var i = 0; i < Dialog.GetLineCount(); i++)
		{
			var range = Dialog.GetLineRange(i);
			var count = range.Y - range.X;
			var duration = count * _timeSpentPerCharacter;
			visibleCharacters += count;
			tween.TweenProperty(Dialog, "visible_characters", visibleCharacters, duration);
			tween.TweenProperty(Dialog, "visible_characters", visibleCharacters, _timeWaitedBetweenLines);
		}
		_tween.Finished += _tween.Kill;
	}

	/// <summary>
	/// Unused
	/// </summary>
	public void HideDialogBox()
	{
		SetFocusMode(FocusModeEnum.None);
		Title.SetText("");
		Dialog.SetText("");
		Dialog.SetVisibleCharacters(-1);
		SetVisible(false);
	}
}