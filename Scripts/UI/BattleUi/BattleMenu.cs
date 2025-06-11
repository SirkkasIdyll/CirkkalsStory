using Godot;

namespace CS.Scripts.UI.BattleUi;

public partial class BattleMenu : ItemList
{
	private const double ReappearAfterTimeDefault = 0.5;
	private double _reappearAfterTime = 0.5;
	
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
		if (Visible)
			return;
		
		if (_reappearAfterTime >= 0)
			_reappearAfterTime -= delta;

		if (_reappearAfterTime <= 0)
			Visible = true;
		
		if (Visible)
			_reappearAfterTime = ReappearAfterTimeDefault;
	}

	private void OnItemActivated(long index)
	{
		/*if (index == 0)
		{
			var player = GetNode<Player>("/root/MainGameScene/Player");
			var mob = GetNode<Mob>("/root/MainGameScene/Mob");
		}*/
		Visible = false;
	}

	private void OnVisibilityChanged()
	{
		if (Visible)
			GrabFocus();
	}
}