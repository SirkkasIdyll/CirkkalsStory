using CS.Components.Damageable;
using Godot;

namespace CS.SlimeFactory.Signals;

public partial class SignalBus
{
    public delegate void HealthAlteredSignalHandler(Node<HealthComponent> node, ref HealthAlteredSignal args);
    public event HealthAlteredSignalHandler? HealthAlteredSignal;
    public void EmitHealthAlteredSignal(Node<HealthComponent> node, ref HealthAlteredSignal args)
    {
        HealthAlteredSignal?.Invoke(node, ref args);
    }
    
    public delegate void MobDiedSignalHandler(Node node, ref MobDiedSignal args);
    public event MobDiedSignalHandler? MobDiedSignal;
    public void EmitMobDiedSignal(Node node, ref MobDiedSignal args)
    {
        MobDiedSignal?.Invoke(node, ref args);
    }
}