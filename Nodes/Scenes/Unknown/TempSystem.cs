using CS.Components.Description;
using CS.Components.Grid;
using CS.Components.Player;
using CS.SlimeFactory;
using Godot;

namespace CS.Nodes.Scenes.Unknown;

public partial class TempSystem : Node2D
{
	private readonly NodeManager _nodeManager = NodeManager.Instance;
	private readonly NodeSystemManager _nodeSystemManager = NodeSystemManager.Instance;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (!_nodeSystemManager.TryGetNodeSystem<PlayerManagerSystem>(out var playerManagerSystem))
			return;

		if (!_nodeManager.TrySpawnNode("MobPlayer", out var node))
			return;

		if (!_nodeSystemManager.TryGetNodeSystem<GridCoordinateSystem>(out var gridSystem))
			return;
		
		playerManagerSystem.SetPlayer(node);
		AddChild(node);
		var camera = new Camera2D();
		camera.SetZoom(new Vector2(2f, 2f));
		node.AddChild(camera);
		gridSystem.SetPosition(playerManagerSystem.GetPlayer(), new Vector2(10, 10));
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		
		if (!_nodeSystemManager.TryGetNodeSystem<GridCoordinateSystem>(out var gridSystem))
			return;
		
		if (!_nodeSystemManager.TryGetNodeSystem<PlayerManagerSystem>(out var playerManagerSystem))
			return;
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);

		if (@event.IsActionPressed("zoom_in"))
		{
			var tween = CreateTween();
			tween.SetEase(Tween.EaseType.InOut);
			tween.TweenProperty(GetViewport().GetCamera2D(), "zoom", new Vector2(3f, 3f), 0.175f);
		}

		if (@event.IsActionPressed("zoom_out"))
		{
			var tween = CreateTween();
			tween.SetEase(Tween.EaseType.InOut);
			tween.TweenProperty(GetViewport().GetCamera2D(), "zoom", new Vector2(2f, 2f), 0.175f);
		}
	}
}