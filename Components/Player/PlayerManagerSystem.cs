using CS.SlimeFactory;
using Godot;

namespace CS.Components.Player;

// TODO: I don't exactly know where to put this besides this, figure out something more sane later
public partial class PlayerManagerSystem : NodeSystem
{
    private Node? _player;

    /// <summary>
    /// TODO: HUNT DOWN ALL USAGES OF THIS AND UNCURSE IT
    /// UNCURSE IT BY MAKING SESSIONS A THING, ALTHOUGH THIS IS KIND OF A SESSION
    /// NO NULL SAFETY, NOTHING IS GOOD ABOUT THIS
    /// 
    /// </summary>
    public Node? TryGetPlayer()
    {
        return _player;
    }

    public void SetPlayer(Node player)
    {
        _player = player;
    }
}

