using CS.Components;
using CS.Components.Damageable;
using CS.Components.Description;
using Godot;

namespace CS.Nodes.Combat;

public partial class NameAndHealthBarSceneSystem : BoxContainer
{
	[ExportCategory("Instantiated")]
	[Export] private Node? _mob;
	
	[ExportCategory("Owned")]
	[Export] private Label? _mobName;
	[Export] private ProgressBar? _mobHpBar;
	[Export] private Label? _mobHpBarNumbers;

	public void SetMob(Node mob)
	{
		_mob = mob;
		
		if (!ComponentSystem.TryGetComponent<DescriptionComponent>(_mob, out var descriptionComponent))
		{
			GD.PrintErr("Mob has no description component\n" + System.Environment.StackTrace);
			return;
		}

		if (!ComponentSystem.TryGetComponent<HealthComponent>(_mob, out var healthComponent))
		{
			GD.PrintErr("Mob has no health component\n" + System.Environment.StackTrace);
			return;
		}

		_mobName?.SetText(descriptionComponent.DisplayName);
		_mobHpBar?.SetMax(healthComponent.MaxHealth);
		_mobHpBar?.SetValue(healthComponent.Health);
		_mobHpBarNumbers?.SetText($"{healthComponent.Health} / {healthComponent.MaxHealth}");
	}
}