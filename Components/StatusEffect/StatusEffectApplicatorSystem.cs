using CS.SlimeFactory;
using Godot;

namespace CS.Components.StatusEffect;

public partial class StatusEffectApplicatorSystem : NodeSystem
{
    public override void _SystemReady()
    {
        base._SystemReady();

        var signal = new StatusEffectSignal();
        SignalBus.AddUserSignal(signal.Name);
        SignalBus.Connect(signal.Name, Callable.From(HIii));
        SignalBus.EmitSignal(signal.Name);
    }

    private void HIii()
    {
        GD.Print("HAIII");
    }
}