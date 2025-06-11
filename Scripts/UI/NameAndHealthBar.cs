using CS.Scripts.Components.Damageable;
using CS.Scripts.Components.Description;
using Godot;

namespace CS.Scripts.UI;

public partial class NameAndHealthBar : BoxContainer
{
	[Export] private CharacterBody2D _mob;
	[Export] private Label _mobName;
	[Export] private ProgressBar _mobHpBar;
	[Export] private Label _mobHpBarNumbers;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var nameComponent = _mob.GetNodeOrNull<NameComponent>("NameComponent");
		var healthComponent = _mob.GetNodeOrNull<HealthComponent>("HealthComponent");

		_mobName.Text = nameComponent != null ? nameComponent.EntityName : ">)]*=`-!#})`/-|[%~";
		if (healthComponent != null)
		{
			_mobHpBar.MaxValue = healthComponent.MaxHealth;
			_mobHpBar.Value = healthComponent.Health;
			_mobHpBarNumbers.Text = $"{healthComponent.Health} / {healthComponent.MaxHealth}";
		}
		else
		{
			_mobHpBar.MaxValue = 1;
			_mobHpBar.Value = 1;
			_mobHpBarNumbers.Text = "NaN / NaN";
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}