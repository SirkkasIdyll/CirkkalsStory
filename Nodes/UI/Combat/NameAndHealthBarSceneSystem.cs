using CS.Components.Damageable;
using CS.Components.Description;
using CS.SlimeFactory;
using Godot;

namespace CS.Nodes.UI.Combat;

public partial class NameAndHealthBarSceneSystem : BoxContainer
{
	private readonly NodeManager _nodeManager = NodeManager.Instance;
	
	[ExportCategory("Instantiated")]
	[Export] private Node? _mob;
	
	[ExportCategory("Owned")]
	[Export] private Label? _mobName;
	[Export] private ProgressBar? _mobHpBar;
	[Export] private Label? _mobHpBarNumbers;
	[Export] private Button? _target;

	[Signal]
	public delegate void TargetChosenEventHandler(Node mob);

	public override void _Ready()
	{
		if (_target != null)
			_target.Pressed += OnTargetPressed;
	}

	private void OnHealthAltered(Node<HealthComponent> node, ref HealthAlteredSignal args)
	{
		if (_mob != node.ParentNode)
			return;
		
		_mobHpBar?.SetValue(node.Component.Health);
		_mobHpBarNumbers?.SetText($"{node.Component.Health} / {node.Component.MaxHealth}");
	}
	
	private void OnTargetPressed()
	{
		EmitSignalTargetChosen(_mob);
		_target?.SetVisible(false);
	}

	public void SetMob(Node mob)
	{
		_mob = mob;
		
		if (!NodeManager.Instance.TryGetComponent<DescriptionComponent>(_mob, out var descriptionComponent))
		{
			GD.PrintErr("Mob has no description component\n" + System.Environment.StackTrace);
			return;
		}

		if (!NodeManager.Instance.TryGetComponent<HealthComponent>(_mob, out var healthComponent))
		{
			GD.PrintErr("Mob has no health component\n" + System.Environment.StackTrace);
			return;
		}

		_mobName?.SetText(descriptionComponent.DisplayName);
		_mobHpBar?.SetMax(healthComponent.MaxHealth);
		_mobHpBar?.SetValue(healthComponent.Health);
		_mobHpBarNumbers?.SetText($"{healthComponent.Health} / {healthComponent.MaxHealth}");
		_nodeManager.SignalBus.HealthAlteredSignal += OnHealthAltered;
	}

	public void ShowTargetButton()
	{
		_target?.SetVisible(true);
		_target?.GrabFocus();
	}

	public void HideTargetButton()
	{
		_target?.SetVisible(false);
	}
}