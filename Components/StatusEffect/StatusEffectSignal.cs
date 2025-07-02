using CS.SlimeFactory;
using Godot;

namespace CS.Components.StatusEffect;

public partial class StatusEffectSignal : UserSignalArgs
{
    public string StatusEffect = "";

    public StatusEffectSignal(Node node) : base(node)
    {
    }
}