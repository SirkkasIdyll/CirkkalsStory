using CS.Components.Damageable;
using CS.Components.Description;
using CS.Components.Magic;
using CS.Components.Mob;
using CS.Components.StatusEffect;
using CS.Nodes.UI.ButtonTypes;
using CS.SlimeFactory;
using Godot;

namespace CS.Nodes.Scenes.Combat;

public partial class CombatMobRepresentationSystem : Control
{
	private readonly NodeManager _nodeManager = NodeManager.Instance;
	
	[ExportCategory("Instantiated")]
	private Node? _mob;

	[ExportCategory("Owned")]
	[Export] public StandardLinkButton MobNameLinkButton = default!;
	[Export] private AnimatedSprite2D _cursor = default!;
	[Export] public ProgressBar HpProgressBar = default!;
	[Export] private Label _hpLabel = default!;
	[Export] private ProgressBar _mpProgressBar = default!;
	[Export] private Label _mpLabel = default!;

	[Signal]
	public delegate void TargetPressedEventHandler(Node mob);
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_nodeManager.SignalBus.HealthAlteredSignal += OnHealthAltered;
		_nodeManager.SignalBus.ManaAlteredSignal += OnManaAltered;
		
		MobNameLinkButton.FocusEntered += OnFocusEntered;
		MobNameLinkButton.FocusExited += OnFocusExited;
		MobNameLinkButton.Pressed += OnPressed;
		MobNameLinkButton.MouseEntered += OnMouseEntered;
		MobNameLinkButton.SetDisabled(true);
		_cursor.SetVisible(false);

		PositionCursor();
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		_nodeManager.SignalBus.HealthAlteredSignal -= OnHealthAltered;
		_nodeManager.SignalBus.ManaAlteredSignal -= OnManaAltered;
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
		HpProgressBar.Value = (double)node.Comp.Health / node.Comp.MaxHealth * 100;
	}

	private void OnManaAltered(Node<ManaComponent> node, ref ManaAlteredSignal args)
	{
		if (node.Owner != _mob)
			return;

		_mpLabel.Text = node.Comp.Mana + " / " + node.Comp.MaxMana;
		_mpProgressBar.Value = (double)node.Comp.Mana / node.Comp.MaxMana * 100;
	}

	private void OnMouseEntered()
	{
		MobNameLinkButton.TooltipText = "";
		
		if (_mob == null)
			return;

		if (!_nodeManager.TryGetComponent<MobComponent>(_mob, out var mobComponent))
			return;

		foreach (var statusEffect in mobComponent.StatusEffects)
		{
			if (!_nodeManager.TryGetComponent<StatusEffectComponent>(statusEffect.Value, out var statusEffectComponent))
				return;

			MobNameLinkButton.TooltipText = statusEffect.Key + ": " + statusEffectComponent.StatusDuration + " turns remaining";
		}
	}

	private void OnPressed()
	{
		if (MobNameLinkButton.Disabled)
			return;
		
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
		
		if (!_nodeManager.TryGetComponent<ManaComponent>(_mob, out var manaComponent))
			return;

		MobNameLinkButton.Text = descriptionComponent.DisplayName;
		_hpLabel.Text = healthComponent.Health + " / " + healthComponent.MaxHealth;
		HpProgressBar.Value = (double) healthComponent.Health / healthComponent.MaxHealth * 100;

		if (manaComponent.MaxMana == 0)
		{
			_mpLabel.GetParent<Control>().SetVisible(false);
			return;
		}
		
		_mpLabel.Text = manaComponent.Mana + " / " + manaComponent.MaxMana;
		_mpProgressBar.Value = (double)manaComponent.Mana / manaComponent.MaxMana * 100;
	}

	private void PositionCursor()
	{
		var position = MobNameLinkButton.GetPosition();
		position.X -= 15;
		position.Y += 20;
		_cursor.SetPosition(position);
	}
}