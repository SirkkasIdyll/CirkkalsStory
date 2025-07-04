using CS.Components.Skills;

namespace CS.SlimeFactory.Signals;

public partial class SignalBus
{
    public delegate void UseSkillSignalHandler(Node<SkillComponent> node, ref UseSkillSignal args);
    public event UseSkillSignalHandler? UseSkillSignal;
    public void EmitUseSkillSignal(Node<SkillComponent> node, ref UseSkillSignal args)
    {
        UseSkillSignal?.Invoke(node, ref args);
    }
}