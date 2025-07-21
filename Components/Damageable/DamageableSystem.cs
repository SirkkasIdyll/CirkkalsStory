using CS.Components.Ability;
using CS.Components.Damage;
using CS.Components.Mob;
using CS.SlimeFactory;

namespace CS.Components.Damageable;

public partial class DamageableSystem : NodeSystem
{
    [InjectDependency] private readonly AbilitySystem _abilitySystem = null!;
    
    public override void _Ready()
    {
        base._Ready();

        _nodeManager.SignalBus.HealthAlteredSignal += OnHealthAltered;
    }

    private void OnHealthAltered(Node<HealthComponent> node, ref HealthAlteredSignal args)
    {
        if (node.Comp.Health <= 0)
        {
            var signal = new MobDiedSignal();
            _nodeManager.SignalBus.EmitMobDiedSignal(node, ref signal);
        }
    }
    
    /// <summary>
    /// Try to make the node take damage
    /// </summary>
    /// <param name="node">The mob taking damage</param>
    /// <param name="attack">The attack being aimed at the target</param>
    /// <param name="preview">When true, does not actually deal damage, only previews the damage</param>
    /// <param name="damageTaken">How much damage is taken after calculations</param>
    public void TryTakeDamage(Node<HealthComponent> node, Node<DamageComponent> attack, bool preview, out int damageTaken)
    {
        damageTaken = 0;
        var damage = attack.Comp.Damage;
        
        // Can't damage a target that isn't damageable
        if (!_nodeManager.TryGetComponent<DamageableComponent>(node, out var damageableComponent))
            return;
        
        // Modify damage based on damage category resistance
        damageableComponent.DamageCategoryResistance.TryGetValue(attack.Comp.DamageCategory,
            out var damageCategoryResistance);
        damage *= damageCategoryResistance;
        
        // Modify damage based on damage type resistance
        damageableComponent.DamageTypeResistance.TryGetValue(attack.Comp.DamageType, out var damageTypeResistance);
        damage *= damageTypeResistance;

        if (_nodeManager.TryGetComponent<MobComponent>(node, out var mobComponent))
        {
            var multiplier = GetDamageResistMultiplier((node, mobComponent), attack.Comp.DamageCategory,
                attack.Comp.DamageType);
            var flat = GetFlatDamageResist((node, mobComponent), attack.Comp.DamageCategory, attack.Comp.DamageType);
            
            damage = float.Max(damage / multiplier - flat, 0);
        }

        damageTaken = (int)damage;
        
        if (preview)
        {
            var previewSignal = new PreviewHealthAlteredSignal(-damageTaken);
            _nodeManager.SignalBus.EmitPreviewHealthAlteredSignal(node, ref previewSignal);
            return;
        }
        
        node.Comp.Health -= (int)damage;
        var signal = new HealthAlteredSignal();
        _nodeManager.SignalBus.EmitHealthAlteredSignal(node, ref signal);
    }
    
        /// <summary>
    /// Try to make the node take damage
    /// </summary>
    /// <param name="node">The mob taking damage</param>
    /// <param name="attack">The attack being aimed at the target</param>
    /// <param name="preview">When true, does not actually deal damage, only previews the damage</param>
    /// <param name="damageTaken">How much damage is taken after calculations</param>
    public void TryTakeDamage(Node<HealthComponent> node, Node<PercentageDamageComponent> attack, bool preview, out int damageTaken)
    {
        damageTaken = 0;
        var damage = node.Comp.MaxHealth * (attack.Comp.PercentDamage / 100);
        
        // Can't damage a target that isn't damageable
        if (!_nodeManager.TryGetComponent<DamageableComponent>(node, out var damageableComponent))
            return;
        
        // Modify damage based on damage category resistance
        damageableComponent.DamageCategoryResistance.TryGetValue(attack.Comp.DamageCategory,
            out var damageCategoryResistance);
        damage *= damageCategoryResistance;
        
        // Modify damage based on damage type resistance
        damageableComponent.DamageTypeResistance.TryGetValue(attack.Comp.DamageType, out var damageTypeResistance);
        damage *= damageTypeResistance;

        if (_nodeManager.TryGetComponent<MobComponent>(node, out var mobComponent))
        {
            var multiplier = GetDamageResistMultiplier((node, mobComponent), attack.Comp.DamageCategory,
                attack.Comp.DamageType);
            var flat = GetFlatDamageResist((node, mobComponent), attack.Comp.DamageCategory, attack.Comp.DamageType);
            
            damage = float.Max(damage / multiplier - flat, 0);
        }

        damageTaken = (int)damage;
        
        if (preview)
        {
            var previewSignal = new PreviewHealthAlteredSignal(-damageTaken);
            _nodeManager.SignalBus.EmitPreviewHealthAlteredSignal(node, ref previewSignal);
            return;
        }
        
        node.Comp.Health -= (int)damage;
        var signal = new HealthAlteredSignal();
        _nodeManager.SignalBus.EmitHealthAlteredSignal(node, ref signal);
    }

    public float GetDamageResistMultiplier(Node<MobComponent> node, DamageCategory damageCategory, DamageType damageType)
    {
        var multiplier = 1f;
        
        // Check mob's abilities for damage resist multiplier
        foreach (var abilityName in node.Comp.Abilities)
        {
            if (!_abilitySystem.TryGetAbility(abilityName, out var abilityNode))
                continue;

            if (!_nodeManager.TryGetComponent<DamageResistMultiplierComponent>(abilityNode,
                    out var damageResistMultiplierComponent))
                continue;
            
            if (damageResistMultiplierComponent.DamageCategoryResistanceMultiplier.TryGetValue(
                    damageCategory, out var categoryMultiplier))
                multiplier *= categoryMultiplier;

            if (damageResistMultiplierComponent.DamageTypeResistanceMultiplier.TryGetValue(
                    damageType, out var typeMultiplier))
                multiplier *= typeMultiplier;
        }
        
        // Check mob's status effects for damage resist multiplier
        foreach (var statusEffect in node.Comp.StatusEffects.Values)
        {
            if (!_nodeManager.TryGetComponent<DamageResistMultiplierComponent>(statusEffect,
                    out var damageResistMultiplierComponent))
                continue;
            
            if (damageResistMultiplierComponent.DamageCategoryResistanceMultiplier.TryGetValue(
                    damageCategory, out var categoryMultiplier))
                multiplier *= categoryMultiplier;

            if (damageResistMultiplierComponent.DamageTypeResistanceMultiplier.TryGetValue(
                    damageType, out var typeMultiplier))
                multiplier *= typeMultiplier;
        }
        
        return multiplier;
    }
    
    public float GetDamageResistMultiplier(Node<MobComponent> node, DamageCategory damageCategory)
    {
        var multiplier = 1f;
        
        // Check mob's abilities for damage resist multiplier
        foreach (var abilityName in node.Comp.Abilities)
        {
            if (!_abilitySystem.TryGetAbility(abilityName, out var abilityNode))
                continue;

            if (!_nodeManager.TryGetComponent<DamageResistMultiplierComponent>(abilityNode,
                    out var damageResistMultiplierComponent))
                continue;
            
            if (damageResistMultiplierComponent.DamageCategoryResistanceMultiplier.TryGetValue(
                    damageCategory, out var categoryMultiplier))
                multiplier *= categoryMultiplier;
        }
        
        // Check mob's status effects for damage resist multiplier
        foreach (var statusEffect in node.Comp.StatusEffects.Values)
        {
            if (!_nodeManager.TryGetComponent<DamageResistMultiplierComponent>(statusEffect,
                    out var damageResistMultiplierComponent))
                continue;
            
            if (damageResistMultiplierComponent.DamageCategoryResistanceMultiplier.TryGetValue(
                    damageCategory, out var categoryMultiplier))
                multiplier *= categoryMultiplier;
        }
        
        return multiplier;
    }
    
    public float GetDamageResistMultiplier(Node<MobComponent> node, DamageType damageType)
    {
        var multiplier = 1f;
        
        // Check mob's abilities for damage resist multiplier
        foreach (var abilityName in node.Comp.Abilities)
        {
            if (!_abilitySystem.TryGetAbility(abilityName, out var abilityNode))
                continue;

            if (!_nodeManager.TryGetComponent<DamageResistMultiplierComponent>(abilityNode,
                    out var damageResistMultiplierComponent))
                continue;

            if (damageResistMultiplierComponent.DamageTypeResistanceMultiplier.TryGetValue(
                    damageType, out var typeMultiplier))
                multiplier *= typeMultiplier;
        }
        
        // Check mob's status effects for damage resist multiplier
        foreach (var statusEffect in node.Comp.StatusEffects.Values)
        {
            if (!_nodeManager.TryGetComponent<DamageResistMultiplierComponent>(statusEffect,
                    out var damageResistMultiplierComponent))
                continue;

            if (damageResistMultiplierComponent.DamageTypeResistanceMultiplier.TryGetValue(
                    damageType, out var typeMultiplier))
                multiplier *= typeMultiplier;
        }
        
        return multiplier;
    }

    public float GetFlatDamageResist(Node<MobComponent> node, DamageCategory damageCategory, DamageType damageType)
    {
        var flatDamageResist = 0f;
        
        // Check mob's abilities for flat damage resist
        foreach (var abilityName in node.Comp.Abilities)
        {
            if (!_abilitySystem.TryGetAbility(abilityName, out var abilityNode))
                continue;

            if (!_nodeManager.TryGetComponent<FlatDamageResistComponent>(abilityNode,
                    out var flatDamageResistComponent))
                continue;
            
            if (flatDamageResistComponent.FlatDamageCategoryResistance.TryGetValue(
                    damageCategory, out var categoryFlat))
                flatDamageResist += categoryFlat;
            
            if (flatDamageResistComponent.FlatDamageTypeResistance.TryGetValue(
                    damageType, out var typeFlat))
                flatDamageResist += typeFlat;
        }
        
        // Check mob's status effects for flat damage resist
        foreach (var statusEffect in node.Comp.StatusEffects.Values)
        {
            if (!_nodeManager.TryGetComponent<FlatDamageResistComponent>(statusEffect,
                    out var flatDamageResistComponent))
                continue;
            
            if (flatDamageResistComponent.FlatDamageCategoryResistance.TryGetValue(
                    damageCategory, out var categoryFlat))
                flatDamageResist += categoryFlat;
            
            if (flatDamageResistComponent.FlatDamageTypeResistance.TryGetValue(
                    damageType, out var typeFlat))
                flatDamageResist += typeFlat;
        }

        return flatDamageResist;
    }
    
    public float GetFlatDamageResist(Node<MobComponent> node, DamageCategory damageCategory)
    {
        var flatDamageResist = 0f;
        
        // Check mob's abilities for flat damage resist
        foreach (var abilityName in node.Comp.Abilities)
        {
            if (!_abilitySystem.TryGetAbility(abilityName, out var abilityNode))
                continue;

            if (!_nodeManager.TryGetComponent<FlatDamageResistComponent>(abilityNode,
                    out var flatDamageResistComponent))
                continue;
            
            if (flatDamageResistComponent.FlatDamageCategoryResistance.TryGetValue(
                    damageCategory, out var categoryFlat))
                flatDamageResist += categoryFlat;
        }
        
        // Check mob's status effects for flat damage resist
        foreach (var statusEffect in node.Comp.StatusEffects.Values)
        {
            if (!_nodeManager.TryGetComponent<FlatDamageResistComponent>(statusEffect,
                    out var flatDamageResistComponent))
                continue;
            
            if (flatDamageResistComponent.FlatDamageCategoryResistance.TryGetValue(
                    damageCategory, out var categoryFlat))
                flatDamageResist += categoryFlat;
        }

        return flatDamageResist;
    }
    
    public float GetFlatDamageResist(Node<MobComponent> node, DamageType damageType)
    {
        var flatDamageResist = 0f;
        
        // Check mob's abilities for flat damage resist
        foreach (var abilityName in node.Comp.Abilities)
        {
            if (!_abilitySystem.TryGetAbility(abilityName, out var abilityNode))
                continue;

            if (!_nodeManager.TryGetComponent<FlatDamageResistComponent>(abilityNode,
                    out var flatDamageResistComponent))
                continue;
            
            if (flatDamageResistComponent.FlatDamageTypeResistance.TryGetValue(
                    damageType, out var typeFlat))
                flatDamageResist += typeFlat;
        }
        
        // Check mob's status effects for flat damage resist
        foreach (var statusEffect in node.Comp.StatusEffects.Values)
        {
            if (!_nodeManager.TryGetComponent<FlatDamageResistComponent>(statusEffect,
                    out var flatDamageResistComponent))
                continue;
            
            if (flatDamageResistComponent.FlatDamageTypeResistance.TryGetValue(
                    damageType, out var typeFlat))
                flatDamageResist += typeFlat;
        }

        return flatDamageResist;
    }
}