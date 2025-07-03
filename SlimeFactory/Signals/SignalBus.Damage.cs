using CS.Components.Damage;

namespace CS.SlimeFactory.Signals;

public partial class SignalBus
{
    public delegate void DamageAttemptSignalHandler(Node<DamageComponent> node, DamageAttemptSignal args);
    public event DamageAttemptSignalHandler DamageAttemptSignal;
    
    public void EmitDamageAttemptSignal(Node<DamageComponent> node, DamageAttemptSignal args)
    {
        DamageAttemptSignal.Invoke(node, args);
    }
}