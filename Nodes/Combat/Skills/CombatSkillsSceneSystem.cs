using CS.Components;
using CS.Components.Mob;
using Godot;
using Godot.Collections;

namespace CS.Nodes.Combat.Skills;

public partial class CombatSkillsSceneSystem : Control
{
	[Export] public CharacterBody2D? Player;
	[Export] private CombatSkillsItemListSystem? _combatSkillsItemListSystem;

	[Signal]
	public delegate void DisplayPlayerSkillsEventHandler(Array<string> skills);

	[Signal]
	public delegate void NoCombatCapabilitiesEventHandler();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		DisplaySkills();
	}


	private void DisplaySkills()
	{
		// We should really always have a player, though
		if (Player == null)
			return;

		// If the player isn't even a mob, they're just incapable of helping
		if (!ComponentSystem.TryGetComponent<MobComponent>(Player, out var mobComponent))
		{
			EmitSignal(SignalName.NoCombatCapabilities);
			return;
		}
		
		// Otherwise we're good to go
		EmitSignal(SignalName.DisplayPlayerSkills, mobComponent.Skills);
	}
}