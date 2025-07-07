using Godot;

namespace CS.Nodes.Scenes.Combat.SkillSelection;

/// <summary>
/// TODO: EXTREMELY LATER ON REFACTOR, USE BUTTONS INSTEAD OF ITEMLIST, EACH BUTTON WILL HOLD THE SKILL
/// </summary>
public partial class CombatSkillSelectionItemListSystem : ItemList
{
	[Signal]
	public delegate void PreviewSkillEventHandler(string skill);
	
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
		EmitSignalPreviewSkill(GetItemText((int) index).Replace(" ", string.Empty));
	}
}