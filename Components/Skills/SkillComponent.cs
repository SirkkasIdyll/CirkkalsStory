using CS.SlimeFactory;
using CS.SlimeFactory.Signals;
using Godot;

namespace CS.Components.Skills;

/// <summary>
/// It's a skill!
/// </summary>
public partial class SkillComponent : Component
{
}

public partial class UseSkillSignal : UserSignalArgs
{
    public UseSkillSignal(Node user, Node? target = null)
    {
        User = user;
        Target = target;
    }

    public Node User;
    public Node? Target;
}