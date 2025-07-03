using CS.Components.Skills;

namespace CS.SlimeFactory.Signals;

public partial class SignalBus
{
    public delegate void UseSkillSignalHandler(Node<SkillComponent> node, UseSkillSignal args);
    public event UseSkillSignalHandler UseSkillSignal;

    public void EmitUseSkillSignal(Node<SkillComponent> node, UseSkillSignal args)
    {
        UseSkillSignal.Invoke(node, args);
    }
}