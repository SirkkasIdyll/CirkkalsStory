using CS.Components.Mob;
using CS.SlimeFactory;
using Godot;

namespace CS.Components.StatusEffect;

public partial class StatusEffectApplicatorComponent : Component
{
    /// <summary>
    /// The status effect to apply to a target
    /// </summary>
    [Export] private Node? _statusEffect;
    
    public void ApplyCombatEffect(Node target)
    {
        if (_statusEffect == null)
            return;
        
        if (!NodeManager.Instance.TryGetComponent<MobComponent>(target, out var mobComponent))
            return;
    }
}