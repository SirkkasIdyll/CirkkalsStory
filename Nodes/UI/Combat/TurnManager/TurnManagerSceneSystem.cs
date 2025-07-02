using Godot;
using Godot.Collections;

namespace CS.Nodes.UI.Combat.TurnManager;

public partial class TurnManagerSceneSystem : Node2D
{
	private Node? _currentMobsTurn;
	private Array<Node> _turnOrder = [];

	/// <summary>
	/// Communicate that it is currently this mob's turn
	/// </summary>
	[Signal]
	public delegate void MobsTurnEventHandler(Node mob);
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	/// <summary>
	/// Moves current mob to last in turn order
	/// Send out a signal for the next mob's turn in list
	/// </summary>
	public void ProceedTurnOrder()
	{
		var moveToLast = _turnOrder[0];
		_turnOrder.RemoveAt(0);
		_turnOrder.Add(moveToLast);
		EmitSignalMobsTurn(_turnOrder[0]);
	}

	public void SetTurnOrder(Array<Node> players, Array<Node> enemies)
	{
		_turnOrder = players + enemies;
		EmitSignalMobsTurn(_turnOrder[0]);
	}

	public void RemoveFromTurnOrder(Node mob)
	{
		_turnOrder.Remove(mob);
	}
}