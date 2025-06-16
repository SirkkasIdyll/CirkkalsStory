using Godot;

namespace CS.Nodes.Battle;

public partial class SkillSelectionItemListSystem : ItemList
{
	[Export] private Label _skillName;
	[Export] private Label _skillEffect;
	[Export] private Label _skillEffect2;
	[Export] private Label _skillDescription;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// GET ALL SKILLS, CHECK FOR SPECIFIC COMPONENTS
		ItemSelected += OnItemSelected;
		Select(0);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnItemSelected(long index)
	{
	}
}