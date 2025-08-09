using CS.SlimeFactory;
using Godot;

namespace CS.Components.Player;

// TODO: I don't exactly know where to put this besides this, figure out something more sane later
public partial class PlayerManagerSystem : NodeSystem
{
    private Node _player = null!;

    /// <summary>
    /// TODO: HUNT DOWN ALL USAGES OF THIS AND UNCURSE IT
    /// UNCURSE IT BY MAKING SESSIONS A THING, ALTHOUGH THIS IS KIND OF A SESSION
    /// 
    /// </summary>
    /// <returns></returns>
    public Node GetPlayer()
    {
        return _player;
    }

    public void SetPlayer(Node player)
    {
        _player = player;
    }
}