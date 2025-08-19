using CS.Components.Description;
using CS.Components.Interaction;
using CS.Components.Player;
using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Nodes.UI.NodeButtonList;

public partial class NodeButtonListSystem : Control
{
	private readonly NodeSystemManager _nodeSystemManager = NodeSystemManager.Instance;
	private readonly NodeManager _nodeManager = NodeManager.Instance;
	
	[ExportCategory("Owned")]
	[Export] private VBoxContainer _vBoxContainer = null!;

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);
		
		if (@event is not InputEventMouseButton mouseEvent)
			return;

		if (!mouseEvent.Pressed)
			return;
		
		if (!new Rect2(Position, _vBoxContainer.Size).HasPoint(GetViewport().GetMousePosition()))
			QueueFree();
	}

	public void Setup(Array<Node2D> node2Ds)
	{
		foreach (var child in _vBoxContainer.GetChildren())
			child.QueueFree();
		
		foreach (var node2D in node2Ds)
		{
			var button = new Button();
			button.SetExpandIcon(true);

			if (_nodeSystemManager.TryGetNodeSystem<DescriptionSystem>(out var descriptionSystem) &&
			    descriptionSystem.TryGetDisplayName(node2D, out var name))
				button.Text = name;
			
			button.Pressed += () =>
			{
				if (!_nodeSystemManager.TryGetNodeSystem<PlayerManagerSystem>(out var playerManagerSystem) ||
				    !_nodeSystemManager.TryGetNodeSystem<InteractSystem>(out var interactSystem) ||
				    !_nodeManager.TryGetComponent<CanInteractComponent>(playerManagerSystem.GetPlayer(), out var canInteractComponent) ||
				    !_nodeManager.TryGetComponent<InteractableComponent>(node2D, out var interactableComponent))
				{
					QueueFree();
					return;
				}
				
				interactSystem.TryInteract((playerManagerSystem.GetPlayer(), canInteractComponent), (node2D, interactableComponent));
				QueueFree();
			};
			_vBoxContainer.AddChild(button);
		}
	}
}