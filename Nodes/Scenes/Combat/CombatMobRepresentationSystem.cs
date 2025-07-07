using CS.Components.Damageable;
using CS.Components.Description;
using CS.SlimeFactory;
using Godot;

namespace CS.Nodes.Scenes.Combat;

public partial class CombatMobRepresentationSystem : Control
{
	private readonly NodeManager _nodeManager = NodeManager.Instance;
	
	[ExportCategory("Instantiated")]
	private Node? _mob;

	[ExportCategory("Owned")]
	[Export] private Label _name = default!;
	[Export] public Button Target = default!;
	[Export] private Sprite2D _cursor = default!;
	[Export] private ProgressBar _hpProgressBar = default!;
	[Export] private Label _hpLabel = default!;

	[Signal]
	public delegate void TargetPressedEventHandler(Node mob);
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_nodeManager.SignalBus.HealthAlteredSignal += OnHealthAltered;
		
		Target.FocusEntered += OnFocusEntered;
		Target.FocusExited += OnFocusExited;
		Target.Pressed += OnTargetPressed;
		Target.SetVisible(false);
		_cursor.SetVisible(false);

		PositionCursor();
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		_nodeManager.SignalBus.HealthAlteredSignal -= OnHealthAltered;
	}

	private void OnFocusEntered()
	{
		_cursor.SetVisible(true);
	}

	private void OnFocusExited()
	{
		_cursor.SetVisible(false);
	}
	
	private void OnHealthAltered(Node<HealthComponent> node, ref HealthAlteredSignal args)
	{
		if (node.Owner != _mob)
			return;
		
		_hpLabel.Text = node.Comp.Health + " / " + node.Comp.MaxHealth;
		_hpProgressBar.Value = (double) node.Comp.Health / node.Comp.MaxHealth * 100;
	}

	private void OnTargetPressed()
	{
		_cursor.SetVisible(false);
		EmitSignalTargetPressed(_mob);
	}

	public void SetMob(Node node)
	{
		_mob = node;
		
		if (!_nodeManager.TryGetComponent<DescriptionComponent>(_mob, out var descriptionComponent))
			return;

		if (!_nodeManager.TryGetComponent<HealthComponent>(_mob, out var healthComponent))
			return;

		_name.Text = descriptionComponent.DisplayName;
		_hpLabel.Text = healthComponent.Health + " / " + healthComponent.MaxHealth;
		_hpProgressBar.Value = (double) healthComponent.Health / healthComponent.MaxHealth * 100;
	}

	private void PositionCursor()
	{
		var position = Target.GetPosition();
		position.X -= 12;
		position.Y += 20;
		_cursor.SetPosition(position);

		var newPos = _cursor.GetPosition();
		newPos.X -= 6;
		var tween = CreateTween().BindNode(this).SetLoops();
		tween.TweenProperty(_cursor, "position", newPos, 0.35);
		tween.TweenProperty(_cursor, "position", _cursor.GetPosition(), 0.35);
	}
}