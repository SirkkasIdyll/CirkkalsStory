using CS.Components.CombatManager;
using CS.Components.Damageable;
using CS.Components.Mob;
using CS.Components.Skills;
using CS.SlimeFactory;
using Godot;

namespace CS.Components.AI;

public partial class EnemyAISystem : NodeSystem
{
    private SkillManagerSystem? _skillManagerSystem;
    
    public override void _Ready()
    {
        base._Ready();

        _nodeManager.SignalBus.EnemyTurnSignal += OnEnemyTurn;

        if (_nodeSystemManager.TryGetNodeSystem<SkillManagerSystem>(out var skillManagerSystem))
            _skillManagerSystem = skillManagerSystem;
    }

    private void OnEnemyTurn(Node<MobComponent> node, ref EnemyTurnSignal args)
    {
        if (!_nodeManager.TryGetComponent<HealthComponent>(node, out var healthComponent))
            return;
        
        // Enemy is dead from a status effect, don't let them continue their turn
        if (healthComponent.Health <= 0)
            return;
        
        if (_skillManagerSystem == null)
            return;

        if (node.Comp.Skills.Count == 0)
            return;
        
        var skillName = node.Comp.Skills.PickRandom();
        if (!_skillManagerSystem.TryGetSkill(skillName, out var skillNode))
            return;

        if (!_nodeManager.TryGetComponent<TargetingComponent>(skillNode, out var targetingComponent))
            return;

        var skillTarget = targetingComponent.ValidTargets == TargetingComponent.Targets.Enemies ? args.Players.PickRandom() : args.Enemies.PickRandom();

        if (!_nodeManager.TryGetComponent<SkillComponent>(skillNode, out var skillComponent))
            return;
        
        var useSkillSignal = new UseSkillSignal(node, skillTarget);
        _nodeManager.SignalBus.EmitUseSkillSignal((skillNode, skillComponent), ref useSkillSignal);
    }
}