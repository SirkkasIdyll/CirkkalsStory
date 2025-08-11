using System.Diagnostics.CodeAnalysis;
using CS.SlimeFactory;
using CS.SlimeFactory.Signals;
using Godot;

namespace CS.Components.Grid;

/// <summary>
/// For the sake of simplification, distance will be assumed to be in tiles
/// A tile is a 32x32 block.
///
/// Negative X is left
/// Negative Y is up
/// Position X is right
/// Positive Y is down
/// (0,0) is the first tile in the positive-positive region
///
/// Does not support having additional grids/maps at the moment
/// </summary>
public partial class GridSystem : NodeSystem
{
    public const float TileSize = 32f;
    
    /// <summary>
    /// Converts from the game's grid position to Godot's GlobalPosition
    /// </summary>
    public Vector2 GridPositionToGlobalPosition(Vector2 gridPosition)
    {
        var centerOfTile = TileSize / 2;
        var x = gridPosition.X * TileSize + centerOfTile;
        var y = gridPosition.Y * TileSize + centerOfTile;
        var globalPosition = new Vector2(x, y);
        return globalPosition;
    }
    
    /// <summary>
    /// Converts from Godot's GlobalPosition to the game's grid positioning system
    /// </summary>
    public Vector2 GlobalPositionToGridPosition(Vector2 globalPosition)
    {
        var centerOfTile = TileSize / 2;
        var x = (globalPosition.X - centerOfTile) / TileSize;
        var y = (globalPosition.Y - centerOfTile) / TileSize;
        var gridPosition = new Vector2(x, y);
        return gridPosition;
    }
    
    /// <summary>
    /// Set the position of the node on the grid
    /// </summary>
    public void SetPosition(Node node, Vector2 gridPosition)
    {
        if (node is not Node2D node2D)
            return;
        
        node2D.SetGlobalPosition(GridPositionToGlobalPosition(gridPosition));
    }

    /// <summary>
    /// Gets the position of the node on the game's grid
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public Vector2? GetPosition(Node node)
    {
        if (node is not Node2D node2D)
            return null;

        return GlobalPositionToGridPosition(node2D.GlobalPosition);
    }

    /// <summary>
    /// Gets the distance between two nodes on the game's grid
    /// </summary>
    public bool TryGetDistance(Node A, Node B, [NotNullWhen(true)] out float? distance)
    {
        distance = null;
        
        if (A is not Node2D node2DA || B is not Node2D node2DB)
            return false;

        var distanceVector = GlobalPositionToGridPosition(node2DA.GlobalPosition) -
                             GlobalPositionToGridPosition(node2DB.GlobalPosition);
        
        distance = distanceVector.Length();
        return true;
    }

    public void AnchorToGrid(Node node)
    {
        if (node is not RigidBody2D rigidBody2D)
            return;

        var position = GetPosition(rigidBody2D);

        if (position == null)
            return;
        
        SetPosition(rigidBody2D, new Vector2(float.Round(position.Value.X), float.Round(position.Value.Y)));
        rigidBody2D.SetGlobalRotationDegrees(GetCardinalAngle(rigidBody2D.GetGlobalRotationDegrees()));
        rigidBody2D.SetFreezeEnabled(true);

        var signal = new NodeAnchoredToGridSignal();
        _nodeManager.SignalBus.EmitNodeAnchoredToGridSignal(rigidBody2D, ref signal);
    }

    public void UnanchorFromGrid(Node node)
    {
        if (node is not RigidBody2D rigidBody2D)
            return;
        
        rigidBody2D.SetFreezeEnabled(false);

        var signal = new NodeUnanchoredFromGridSignal();
        _nodeManager.SignalBus.EmitNodeUnanchoredFromGridSignal(rigidBody2D, ref signal);
        GD.Print("Unanchored " + rigidBody2D.Name);
    }

    public void ToggleAnchored(Node node)
    {
        if (node is not RigidBody2D rigidBody2D)
            return;

        if (rigidBody2D.IsFreezeEnabled())
        {
            UnanchorFromGrid(node);
            return;
        }
        
        AnchorToGrid(node);
    }

    /// <summary>
    /// It's in degrees, not radians!
    /// </summary>
    private float GetCardinalAngle(float degrees)
    {
        return float.Round(degrees / 90) * 90;
    }
}

public partial class NodeAnchoredToGridSignal : UserSignalArgs
{
    
}

public partial class NodeUnanchoredFromGridSignal : UserSignalArgs
{
    
}