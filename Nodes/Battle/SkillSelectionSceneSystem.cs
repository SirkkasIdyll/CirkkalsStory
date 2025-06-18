using CS.Components;
using CS.Components.Mob;
using CS.Nodes.Skills.Manager;
using Godot;
using Godot.Collections;

namespace CS.Nodes.Battle;

public partial class SkillSelectionSceneSystem : Control
{
	public CharacterBody2D? Player;
	private SkillManagerSceneSystem? _skillManagerSceneSystem;
	[Export] private SkillSelectionItemListSystem? _skillSelectionItemListSystem;
	[Signal] public delegate void DisplayPlayerSkillsEventHandler(Array<string> skills);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		VisibilityChanged += OnVisibilityChanged;

		// TODO: Un-hardcode SkillManager
		_skillManagerSceneSystem = GetNode<SkillManagerSceneSystem>("/root/MainScene/SkillManagerScene");
	}
	
	private void OnVisibilityChanged()
	{
		if (Player == null || _skillManagerSceneSystem == null || _skillSelectionItemListSystem == null)
			return;

		if (!ComponentSystem.TryGetComponent<MobComponent>(Player, out var mobComponent))
			return;

		EmitSignal(SignalName.DisplayPlayerSkills, mobComponent.Skills);
	}
}