using Godot;

namespace CS.Nodes.Battle;

public partial class ActionsItemList : ItemList
{
	private const double ReappearAfterTimeDefault = 0.5;
	private double _reappearAfterTime = 0.5;
	[Export] private SkillSelectionSceneSystem? _skillScene;
	[Export] private CharacterBody2D? _activePlayer; 
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ItemActivated += OnItemActivated;
		VisibilityChanged += OnVisibilityChanged;
		
		CallDeferred(Control.MethodName.GrabFocus);
		Select(0);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		/*if (Visible)
			return;
		
		if (_reappearAfterTime >= 0)
			_reappearAfterTime -= delta;

		if (_reappearAfterTime <= 0)
			Visible = true;
		
		if (Visible)
			_reappearAfterTime = ReappearAfterTimeDefault;*/
	}

	private void OnItemActivated(long index)
	{
		switch (index)
		{
			// Skills
			case 0:
			{
				Visible = false;
			
				if (_skillScene != null)
				{
					_skillScene.Player = _activePlayer;
					_skillScene.Visible = true;
				}

				break;
			}
			// Spells
			case 1:
				break;
			// Flee
			case 2:
				GetTree().Quit();
				break;
		}
	}

	private void OnVisibilityChanged()
	{
		if (Visible)
			GrabFocus();
	}
}