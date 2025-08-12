using CS.Components.CombatManager;
using CS.Components.Damage;
using CS.Components.Damageable;
using CS.Components.Description;
using CS.Components.Mob;
using CS.Components.Skills;
using CS.SlimeFactory;

namespace CS.Components.AI;

public partial class EnemyAISystem : NodeSystem
{
    [InjectDependency] private readonly DescriptionSystem _descriptionSystem = null!;
    [InjectDependency] private readonly SkillSystem _skillManagerSystem = null!;
    
    public override void _Ready()
    {
        base._Ready();

        _nodeManager.SignalBus.EnemyTurnSignal += OnEnemyTurn;
    }

    private void OnEnemyTurn(Node<MobComponent> node, ref EnemyTurnSignal args)
    {
        if (!_nodeManager.TryGetComponent<HealthComponent>(node, out var healthComponent))
        {
            args.Canceled = true;
            return;
        }
        
        // Enemy is dead from a status effect, don't let them continue their turn
        if (healthComponent.Health <= 0)
        {
            args.Canceled = true;
            return;
        }

        if (node.Comp.Skills.Count == 0)
        {
            args.Canceled = true;
            return;
        }
        
        var skillName = node.Comp.Skills.PickRandom();
        if (!_skillManagerSystem.TryGetSkill(skillName, out var skillNode))
        {
            args.Canceled = true;
            return;
        }

        if (!_nodeManager.TryGetComponent<TargetingComponent>(skillNode, out var targetingComponent))
        {
            args.Canceled = true;
            return;
        }

        var skillTarget = targetingComponent.ValidTargets == TargetingComponent.Targets.Enemies ? args.Players.PickRandom() : args.Enemies.PickRandom();
        
        var signal = new UseActionSignal(skillNode, [skillTarget]);
        /*signal.Summaries.Add("Used [b]" + _descriptionSystem.GetDisplayName(skillNode!) + "[/b] on [b]" +
                             _descriptionSystem.GetDisplayName(skillTarget) + "[/b].");*/
        _nodeManager.SignalBus.EmitUseActionSignal(node, ref signal);
        args.Summaries.AddRange(signal.Summaries);
    }
}