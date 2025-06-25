using Godot;

namespace CS.Nodes.Combat;

public partial class ActionsItemList : ItemList
{
	[Export] private PackedScene? _skillSelectionScene;
	[Export] private CharacterBody2D? _activePlayer; 
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ItemActivated += OnItemActivated;
		
		CallDeferred(Control.MethodName.GrabFocus);
		Select(0);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnItemActivated(long index)
	{
	}
}