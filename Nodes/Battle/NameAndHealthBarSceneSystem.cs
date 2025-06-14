using CS.Components.Damageable;
using CS.Components.Description;
using Godot;

namespace CS.Nodes.Battle;

public partial class NameAndHealthBarSceneSystem : BoxContainer
{
	[Export] private CharacterBody2D _mob;
	[Export] private Label _mobName;
	[Export] private ProgressBar _mobHpBar;
	[Export] private Label _mobHpBarNumbers;
	private string _invalidName = ">)]*=`-!#})`/-|[%~";
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var nameComponent = _mob.GetNodeOrNull<DescriptionComponent>("NameComponent");
		var healthComponent = _mob.GetNodeOrNull<HealthComponent>("HealthComponent");

		_mobName.Text = nameComponent != null ? nameComponent.DisplayName : _invalidName;
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
}