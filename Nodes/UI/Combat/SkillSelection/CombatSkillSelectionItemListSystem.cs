using Godot;

namespace CS.Nodes.UI.Combat.SkillSelection;

/// <summary>
/// TODO: EXTREMELY LATER ON REFACTOR, USE BUTTONS INSTEAD OF ITEMLIST, EACH BUTTON WILL HOLD THE SKILL
/// </summary>
public partial class CombatSkillSelectionItemListSystem : ItemList
{
	[Signal]
	public delegate void PreviewSkillEventHandler(string skill);
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ItemSelected += OnItemSelected;
	}

	public void OnItemSelected(long index)
	{
		EmitSignalPreviewSkill(GetItemText((int) index));
	}
}