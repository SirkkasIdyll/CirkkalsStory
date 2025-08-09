using CS.Components.CombatManager;
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

	private const float TimeToUpdateBar = 0.15f;
	
	[ExportCategory("Instantiated")]
	private Node? _mob;

	[ExportCategory("Owned")]
	[Export] public StandardLinkButton MobNameLinkButton = null!;
	[Export] private AnimatedSprite2D _cursor = null!;
	[Export] public ProgressBar HpProgressBar = null!;
	[Export] private Label _hpLabel = null!;
	[Export] private ProgressBar _mpProgressBar = null!;
	[Export] private Label _mpLabel = null!;

	[Signal]
	public delegate void TargetPreviewEventHandler(Node mob);
	
	[Signal]
	public delegate void TargetPressedEventHandler(Node mob);
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_nodeManager.SignalBus.HealthAlteredSignal += OnHealthAltered;
		_nodeManager.SignalBus.PreviewHealthAlteredSignal += OnPreviewHealthAltered;
		_nodeManager.SignalBus.ManaAlteredSignal += OnManaAltered;
		_nodeManager.SignalBus.PreviewManaAlteredSignal += OnPreviewManaAltered;
		
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
		_nodeManager.SignalBus.PreviewHealthAlteredSignal -= OnPreviewHealthAltered;
		_nodeManager.SignalBus.ManaAlteredSignal -= OnManaAltered;
		_nodeManager.SignalBus.PreviewManaAlteredSignal -= OnPreviewManaAltered;
	}

	/// <summary>
	/// Tells the <see cref="CombatSceneSystem"/> to preview the action on this target
	/// </summary>
	private void OnFocusEntered()
	{
		_cursor.SetVisible(true);
		EmitSignalTargetPreview(_mob);
	}

	/// <summary>
	/// Clear the preview and reset health back to normal
	/// </summary>
	private void OnFocusExited()
	{
		_cursor.SetVisible(false);

		if (!_nodeManager.TryGetComponent<HealthComponent>(_mob!, out var healthComponent))
			return;
		
		_hpLabel.Text = healthComponent.Health + " / " + healthComponent.MaxHealth;
		HpProgressBar.Value = (double)healthComponent.Health / healthComponent.MaxHealth * 100;

	}
	
	private void OnHealthAltered(Node<HealthComponent> node, ref HealthAlteredSignal args)
	{
		// if the signal isn't for the mob that this representation is managing, ignore it
		if (node.Owner != _mob)
			return;
		
		_hpLabel.Text = node.Comp.Health + " / " + node.Comp.MaxHealth;
		var tween = CreateTween();
		tween.TweenProperty(HpProgressBar, "value", (double) node.Comp.Health / node.Comp.MaxHealth * 100, TimeToUpdateBar);
	}
	
	/// <summary>
	/// Show potential health change when focused
	/// </summary>
	private void OnPreviewHealthAltered(Node<HealthComponent> node, ref PreviewHealthAlteredSignal args)
	{
		// if the signal isn't for the mob that this representation is managing, ignore it
		if (node.Owner != _mob)
			return;

		var potentialValue = float.Min(float.Max(node.Comp.Health + args.Amount, 0), node.Comp.MaxHealth);
		_hpLabel.Text = potentialValue + " / " + node.Comp.MaxHealth;
		var tween = CreateTween();
		tween.TweenProperty(HpProgressBar, "value", potentialValue / node.Comp.MaxHealth * 100, TimeToUpdateBar);
		MobNameLinkButton.FocusExited += tween.Kill;
	}
	
	private void OnManaAltered(Node<ManaComponent> node, ref ManaAlteredSignal args)
	{
		// if the signal isn't for the mob that this representation is managing, ignore it
		if (node.Owner != _mob)
			return;

		_mpLabel.Text = node.Comp.Mana + " / " + node.Comp.MaxMana;
		var tween = CreateTween();
		tween.TweenProperty(_mpProgressBar, "value", (double) node.Comp.Mana / node.Comp.MaxMana * 100, TimeToUpdateBar);
	}

	/// <summary>
	/// Show potential mana usage when using action
	/// </summary>
	private void OnPreviewManaAltered(Node<ManaComponent> node, ref PreviewManaAlteredSignal args)
	{
		if (node.Owner != _mob)
			return;
		
		var potentialValue = float.Min(float.Max(node.Comp.Mana + args.Amount, 0), node.Comp.MaxMana);
		_mpLabel.Text = potentialValue + " / " + node.Comp.MaxMana;
		var tween = CreateTween();
		tween.TweenProperty(_mpProgressBar, "value", potentialValue / node.Comp.MaxMana * 100, TimeToUpdateBar);
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

	public void CancelPreview()
	{
		if (_nodeManager.TryGetComponent<HealthComponent>(_mob!, out var healthComponent))
		{
			_hpLabel.Text = healthComponent.Health + " / " + healthComponent.MaxHealth;
			HpProgressBar.Value = (double)healthComponent.Health / healthComponent.MaxHealth * 100;
		}

		if (_nodeManager.TryGetComponent<ManaComponent>(_mob!, out var manaComponent))
		{
			_mpLabel.Text = manaComponent.Mana + " / " + manaComponent.MaxMana;
			_mpProgressBar.Value = (double)manaComponent.Mana / manaComponent.MaxMana * 100;
		}
	}
	
	private void PositionCursor()
	{
		var position = MobNameLinkButton.GetPosition();
		position.X -= 15;
		position.Y += 20;
		_cursor.SetPosition(position);
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
		
		if (_mob is Node2D node2D)
			node2D.SetPosition(new Vector2(GlobalPosition.X + GetSize().X / 2, GlobalPosition.Y - node2D.GetNode<Sprite2D>("Sprite2D").Texture.GetHeight() * node2D.GetNode<Sprite2D>("Sprite2D").Scale.Y / 2 - 20));


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
}