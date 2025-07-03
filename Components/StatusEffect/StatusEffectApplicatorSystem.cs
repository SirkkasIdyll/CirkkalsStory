using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Components.StatusEffect;

public partial class StatusEffectApplicatorSystem : NodeSystem
{
    public override void _SystemReady()
    {
        base._SystemReady();
        
        /*var signal = new StatusEffectSignal();
        _nodeManager.SignalBus.AddUserSignal(signal);
        _nodeManager.SignalBus.Connect(signal, this, nameof(HIii));
        _nodeManager.SignalBus.EmitSignal<StatusEffectSignal>(ref signal);*/
    }
    
    public void HIii(Array<Variant> args)
    {
    }
}