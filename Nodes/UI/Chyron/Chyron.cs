using Godot;

namespace CS.Nodes.UI.Chyron;

public partial class Chyron : Control
{
	public Vector2 NorthPosition = new Vector2(640, 380);
	public Vector2 SouthPosition = new Vector2(640, 580);
	
	[ExportCategory("Owned")]
	[Export] private PanelContainer _panelContainer = null!;
	[Export] private Label _label = null!;
	[Export] public Timer Timer = null!;

	public void SetPanel(StyleBox styleBox)
	{
		_panelContainer.AddThemeStyleboxOverride("panel", styleBox);
	}

	public void SetText(string text)
	{
		_label.SetText(text);
	}
}