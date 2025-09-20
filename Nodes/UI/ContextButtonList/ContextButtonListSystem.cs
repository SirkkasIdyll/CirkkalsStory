using CS.Components.Description;
using CS.Components.Interaction;
using CS.Components.Player;
using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Nodes.UI.ContextButtonList;

public partial class ContextButtonListSystem : Control
{
	private readonly NodeSystemManager _nodeSystemManager = NodeSystemManager.Instance;
	private readonly NodeManager _nodeManager = NodeManager.Instance;
	[InjectDependency] private readonly PlayerManagerSystem _playerManagerSystem = null!;
	[InjectDependency] private readonly InteractSystem _interactSystem = null!;
	
	[ExportCategory("Owned")]
	[Export] private VBoxContainer _vBoxContainer = null!;

	public override void _GuiInput(InputEvent @event)
	{
		base._GuiInput(@event);
		
		GetViewport().SetInputAsHandled();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		base._UnhandledInput(@event);
		
		if (@event is not InputEventMouseButton mouseEvent)
			return;

		if (!mouseEvent.Pressed)
			return;
		
		if (!new Rect2(Position, _vBoxContainer.Size).HasPoint(GetViewport().GetMousePosition()))
			QueueFree();
	}

	public void Setup(Array<Node2D> node2Ds)
	{
		_nodeSystemManager.InjectNodeSystemDependencies(this);
		
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
				var player = _playerManagerSystem.TryGetPlayer();
				if (player == null ||
				    !_nodeManager.TryGetComponent<CanInteractComponent>(player, out var canInteractComponent) ||
				    !_nodeManager.TryGetComponent<InteractableComponent>(node2D, out var interactableComponent))
				{
					QueueFree();
					return;
				}

				_interactSystem.TryInteract((player, canInteractComponent), (node2D, interactableComponent));
				QueueFree();
			};
			_vBoxContainer.AddChild(button);
		}
	}

	public void Setup(Array<Button> buttons)
	{
		foreach (var child in _vBoxContainer.GetChildren())
			child.QueueFree();

		foreach (var button in buttons)
		{
			button.Pressed += QueueFree;
			_vBoxContainer.AddChild(button);
		}
	}
}