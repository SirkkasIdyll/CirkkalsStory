using CS.SlimeFactory;
using Godot.Collections;

namespace CS.Components.StatusEffect;

public class StatusEffectSignal : IUserSignal
{
    public string Name
    {
        get => nameof(StatusEffectSignal);
        set { }
    }
}