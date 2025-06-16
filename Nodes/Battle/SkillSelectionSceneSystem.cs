using Godot;

namespace CS.Nodes.Battle;

public partial class SkillSelectionSceneSystem : Control
{
	public CharacterBody2D player;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		VisibilityChanged += OnVisibilityChanged;
	}
	
	private void OnVisibilityChanged()
	{
	}
}