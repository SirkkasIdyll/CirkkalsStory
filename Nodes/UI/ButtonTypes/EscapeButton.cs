using Godot;

namespace PC.Nodes.UI.ButtonTypes;

public partial class EscapeButton : StandardButton
{
    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventKey eventKey && eventKey.IsPressed() && eventKey.Keycode == Key.Escape)
        {
            GetViewport().SetInputAsHandled();
            GetTree().Quit();
        }
    }
}