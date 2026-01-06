using Godot;
using PC.Components.Description;
using PC.Components.Magic.MagicCircle;
using PC.SlimeFactory;

namespace PC.Nodes.Scenes.MagicCircle;

public partial class MagicCircleSceneSystem : VBoxContainer, IModifiableScene
{
	private readonly NodeManager _nodeManager = NodeManager.Instance;
	private readonly NodeSystemManager _nodeSystemManager = NodeSystemManager.Instance;
	[InjectDependency] private readonly MagicCircleSystem _magicCircleSystem = null!;
	[InjectDependency] private readonly DescriptionSystem _descriptionSystem = null!;
	
	public Vector2 DrawPosition = Vector2.Zero;
	public Color LineColor = Colors.Crimson;
	private Node<MagicCircleComponent>? _uiOwner;
    
	public override void _Input(InputEvent @event)
	{
		base._Input(@event);

		if (!@event.IsActionPressed("MAGIC_CIRCLE_TEST"))
			return;

		DrawPosition = GetGlobalMousePosition();
		QueueRedraw();
	}

	public override void _Draw()
	{
		base._Draw();
		
		DrawCircle(Position, 50f, LineColor, false, 10f);
		DrawCircle(Position + new Vector2(15f, 15f), 30f, Colors.Aqua, false, 5f);
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public void ModifyScene(Node node)
	{
		if (!_nodeManager.TryGetComponent<MagicCircleComponent>(node, out var magicCircleComponent))
			return;

		_uiOwner = (node, magicCircleComponent);
	}
}