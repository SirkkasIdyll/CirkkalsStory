using CS.Components.Power;

namespace CS.SlimeFactory.Signals;

public partial class SignalBus
{
    public delegate void PowerGeneratedSignalHandler(Node<PowerGeneratorComponent> node, ref PowerGeneratedSignal args);
    public event  PowerGeneratedSignalHandler? PowerGeneratedSignal;
    public void EmitPowerGeneratedSignal(Node<PowerGeneratorComponent> node, ref PowerGeneratedSignal args)
    {
        PowerGeneratedSignal?.Invoke(node, ref args);
    }
    
    public delegate void PowerTransmittedSignalHandler(Node<PowerTransmissionComponent> node, ref PowerTransmittedSignal args);
    public event  PowerTransmittedSignalHandler? PowerTransmittedSignal;
    public void EmitPowerTransmittedSignal(Node<PowerTransmissionComponent> node, ref PowerTransmittedSignal args)
    {
        PowerTransmittedSignal?.Invoke(node, ref args);
    }
}