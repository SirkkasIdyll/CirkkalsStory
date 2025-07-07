using Godot;

namespace CS.Nodes.Scenes.Combat;

public partial class ActionsItemListSystem : ItemList
{
	[Signal]
	public delegate void SkillsPressedEventHandler();

	[Signal]
	public delegate void SpellsPressedEventHandler();

	[Signal]
	public delegate void FleePressedEventHandler();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ItemActivated += OnItemActivated;
		VisibilityChanged += OnVisibilityChanged;
		
		CallDeferred(Control.MethodName.GrabFocus);
		Select(0);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		base._UnhandledInput(@event);
		
		if (GetViewport().GuiGetFocusOwner() == null && IsVisibleInTree())
			GrabFocus();
	}

	private void OnItemActivated(long index)
	{
		switch (index)
		{
			case 0:
				EmitSignalSkillsPressed();
				break;
			case 1:
				EmitSignalSpellsPressed();
				break;
			case 2:
				EmitSignalFleePressed();
				break;
		}
	}

	private void OnVisibilityChanged()
	{
		if (Visible)
			GrabFocus();
	}
}