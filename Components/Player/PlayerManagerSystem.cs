using CS.SlimeFactory;
using Godot;

namespace CS.Components.Player;

// TODO: I don't exactly know where to put this besides this, figure out something more sane later
public partial class PlayerManagerSystem : NodeSystem
{
    private Node _player = null!;
    
    public override void _Ready()
    {
        base._Ready();

        /*if (_nodeManager.TrySpawnNode("MobPlayer", out var node))
        {
            _player = node;
            AddChild(_player);
            _player.SetOwner(this);
        }*/
    }

    public Node GetPlayer()
    {
        return _player;
    }

    public void SetPlayer(Node player)
    {
        _player = player;
    }
}