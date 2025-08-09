using CS.Components.Description;
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
		
		playerManagerSystem.SetPlayer(node);
		AddChild(node);
		var camera = new Camera2D();
		node.AddChild(camera);
		
		if (node is CharacterBody2D characterBody)
			characterBody.SetPosition(new Vector2(300, 300));
	}
}