using Godot;

namespace CS.Nodes.Scenes.Combat.SpellSelection;

/// <summary>
/// TODO: EXTREMELY LATER ON REFACTOR, USE BUTTONS INSTEAD OF ITEMLIST, EACH BUTTON WILL HOLD THE SPELL
/// </summary>
public partial class SpellSelectionItemListSystem : ItemList
{
	[Signal]
	public delegate void PreviewSpellEventHandler(string spell);
	
	[Signal]
	public delegate void EscapePressedEventHandler();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ItemSelected += OnItemSelected;
	}
	
	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventKey eventKey && eventKey.IsPressed() && eventKey.Keycode == Key.Escape)
		{
			EmitSignalEscapePressed();
			GetViewport().SetInputAsHandled();
		}
	}

	public void OnItemSelected(long index)
	{
		EmitSignalPreviewSpell(GetItemText((int) index).Replace(" ", string.Empty));
	}
}