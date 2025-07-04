using CS.SlimeFactory;

namespace CS.Components.Damageable;

public partial class DamageableSystem : NodeSystem
{
    public override void _Ready()
    {
        base._Ready();

        _nodeManager.SignalBus.HealthAlteredSignal += OnHealthAltered;
    }

    private void OnHealthAltered(Node<HealthComponent> node, ref HealthAlteredSignal args)
    {
        if (node.Component.Health <= 0)
        {
            var signal = new MobDiedSignal();
            _nodeManager.SignalBus.EmitMobDiedSignal(node, ref signal);
        }
    }

    /// <summary>
    /// Try to make the node take damage
    /// </summary>
    /// <param name="node">The mob taking damage</param>
    /// <param name="amount">The amount of damage to inflict on them</param>
    public void TryTakeDamage(Node<HealthComponent> node, int amount)
    {
        // Can't damage a target that isn't damageable
        if (!_nodeManager.HasComponent<DamageableComponent>(node))
            return;

        node.Component.Health -= amount;
        
        var signal = new HealthAlteredSignal();
        _nodeManager.SignalBus.EmitHealthAlteredSignal(node, ref signal);
    }
    
    // TODO: Receive HealthAlteredSignal, check if health hit zero, if so emit mob died signal
}