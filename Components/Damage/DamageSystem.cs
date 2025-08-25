using CS.Components.Ability;
using CS.Components.Damageable;
using CS.Components.Description;
using CS.Components.Mob;
using CS.Components.StatusEffect;
using CS.SlimeFactory;
using Godot;

namespace CS.Components.Damage;

public partial class DamageSystem : NodeSystem
{
    [InjectDependency] private readonly AbilitySystem _abilitySystem = null!;
    [InjectDependency] private readonly DamageableSystem _damageableSystem = null!;
    [InjectDependency] private readonly DescriptionSystem _descriptionSystem = null!;
    
    public override void _Ready()
    {
        base._Ready();

        _nodeManager.SignalBus.GetActionEffectsDescriptionSignal += OnGetActionEffectsDescription;
        _nodeManager.SignalBus.ProcStatusEffectSignal += OnProcStatusEffect;
    }

    private void OnGetActionEffectsDescription(Node<DescriptionComponent> node, ref GetActionEffectsDescriptionSignal args)
    {
        if (_nodeManager.TryGetComponent<DamageComponent>(node, out var damageComponent))
        {
            node.Comp.Effects.Add(damageComponent.DamageCategory.ToString());
            node.Comp.Effects.Add("Type:" + damageComponent.DamageType);
            node.Comp.Effects.Add("Damage: " + damageComponent.Damage);
            return;
        }

        if (_nodeManager.TryGetComponent<PercentageDamageComponent>(node, out var percentageDamageComponent))
        {
            node.Comp.Effects.Add(percentageDamageComponent.DamageCategory.ToString());
            node.Comp.Effects.Add("Type:" + percentageDamageComponent.DamageType);
            node.Comp.Effects.Add("Damage: " + percentageDamageComponent.PercentDamage + "% HP");
            return;
        }
    }

    private void OnProcStatusEffect(Node<StatusEffectComponent> node, ref ProcStatusEffectSignal args)
    {
        if (_nodeManager.TryGetComponent<DamageComponent>(node, out var damageComponent))
        {
            TryDamageTarget((node, damageComponent), args.Target, node, false, out var damageDealt);
            //  args.Summaries.Add("[b]" + _descriptionSystem.GetDisplayName(args.Target) + "[/b] took " + damageDealt + " damage from [b]" + _descriptionSystem.GetDisplayName(node)+ "[/b].");
            return;
        }

        if (_nodeManager.TryGetComponent<PercentageDamageComponent>(node, out var percentageDamageComponent))
        {
            TryDamageTarget((node, percentageDamageComponent), args.Target, node, false, out var damageDealt);
            // args.Summaries.Add("[b]" + _descriptionSystem.GetDisplayName(args.Target) + "[/b] took " + damageDealt + " damage from [b]" + _descriptionSystem.GetDisplayName(node)+ "[/b].");
            return;
        }
    }
    
    /// <summary>
    /// Try to apply damage to a target
    /// </summary>
    /// <param name="node">Skill being used to damage the defender</param>
    /// <param name="defender">The node being attacked</param>
    /// <param name="attacker">The node attacking the defender</param>
    /// <param name="preview">If true, don't actually damage target, just preview the damage</param>
    /// <param name="damageDealt">How much damage is dealt to the target</param>
    public void TryDamageTarget(Node<DamageComponent> node, Node defender, Node attacker, bool preview, out int damageDealt)
    {
        damageDealt = 0;
        var originalDamage = node.Comp.Damage;
        
        // Can't damage a target that has no health
        if (!_nodeManager.TryGetComponent<HealthComponent>(defender, out var healthComponent))
            return;

        // The attacking source can be a status effect, we'll just check if it isn't
        if (_nodeManager.TryGetComponent<MobComponent>(attacker, out var mobComponent))
        {
            node.Comp.Damage *= GetDamageMultiplier((attacker, mobComponent), node.Comp.DamageCategory, node.Comp.DamageType);
            node.Comp.Damage += GetFlatDamageIncrease((attacker, mobComponent), node.Comp.DamageCategory, node.Comp.DamageType);
        }
        
        _damageableSystem.TryTakeDamage((defender, healthComponent), node, preview, out var damageTaken);
        node.Comp.Damage = originalDamage;
        damageDealt = damageTaken;
    }
    
    /// <summary>
    /// Try to apply percentage damage to a target
    /// </summary>
    /// <param name="node">Skill being used to damage the defender</param>
    /// <param name="defender">The node being attacked</param>
    /// <param name="attacker">The node attacking the defender</param>
    /// <param name="preview">If true, don't actually damage target, just preview the damage</param>
    /// <param name="damageDealt">How much damage is dealt to the target</param>
    public void TryDamageTarget(Node<PercentageDamageComponent> node, Node defender, Node attacker, bool preview,
        out int damageDealt)
    {
        damageDealt = 0;
        
        // Can't damage a target that has no health
        if (!_nodeManager.TryGetComponent<HealthComponent>(defender, out var healthComponent))
            return;
        
        _damageableSystem.TryTakeDamage((defender, healthComponent), node, preview, out var damageTaken);
        damageDealt = damageTaken;
    }

    public float GetDamageMultiplier(Node<MobComponent> node, DamageCategory damageCategory, DamageType damageType)
    {
        var multiplier = 1f;
        
        // Check a mob's abilities for any kind of multiplicative damage increase
        foreach (var abilityName in node.Comp.Abilities)
        {
            if (!_abilitySystem.TryGetAbility(abilityName, out var ability))
                continue;

            if (!_nodeManager.TryGetComponent<DamageDealtMultiplierComponent>(ability,
                    out var damageDealtMultiplierComponent))
                continue;
            
            if (damageDealtMultiplierComponent.DamageDealtCategoryMultiplier.TryGetValue(
                    damageCategory, out var categoryMultiplier))
                multiplier *= categoryMultiplier;
            
            if (damageDealtMultiplierComponent.DamageDealtTypeMultiplier.TryGetValue(
                    damageType, out var typeMultiplier))
                multiplier *= typeMultiplier;
        }

        // Check a mob's status effects for any kind of multiplicative damage increase
        foreach (var statusEffect in node.Comp.StatusEffects.Values)
        {
            if (!_nodeManager.TryGetComponent<DamageDealtMultiplierComponent>(statusEffect,
                    out var damageDealtMultiplierComponent))
                continue;
            
            if (damageDealtMultiplierComponent.DamageDealtCategoryMultiplier.TryGetValue(
                    damageCategory, out var categoryMultiplier))
                multiplier *= categoryMultiplier;
            
            if (damageDealtMultiplierComponent.DamageDealtTypeMultiplier.TryGetValue(
                    damageType, out var typeMultiplier))
                multiplier *= typeMultiplier;
        }
        
        return multiplier;
    }
    
    public float GetDamageMultiplier(Node<MobComponent> node, DamageCategory damageCategory)
    {
        var multiplier = 1f;
        
        // Check a mob's abilities for any kind of multiplicative damage increase
        foreach (var abilityName in node.Comp.Abilities)
        {
            if (!_abilitySystem.TryGetAbility(abilityName, out var ability))
                continue;

            if (!_nodeManager.TryGetComponent<DamageDealtMultiplierComponent>(ability,
                    out var damageDealtMultiplierComponent))
                continue;
            
            if (damageDealtMultiplierComponent.DamageDealtCategoryMultiplier.TryGetValue(
                    damageCategory, out var categoryMultiplier))
                multiplier *= categoryMultiplier;
        }

        // Check a mob's status effects for any kind of multiplicative damage increase
        foreach (var statusEffect in node.Comp.StatusEffects.Values)
        {
            if (!_nodeManager.TryGetComponent<DamageDealtMultiplierComponent>(statusEffect,
                    out var damageDealtMultiplierComponent))
                continue;
            
            if (damageDealtMultiplierComponent.DamageDealtCategoryMultiplier.TryGetValue(
                    damageCategory, out var categoryMultiplier))
                multiplier *= categoryMultiplier;
        }
        
        return multiplier;
    }
    
    public float GetDamageMultiplier(Node<MobComponent> node, DamageType damageType)
    {
        var multiplier = 1f;
        
        // Check a mob's abilities for any kind of multiplicative damage increase
        foreach (var abilityName in node.Comp.Abilities)
        {
            if (!_abilitySystem.TryGetAbility(abilityName, out var ability))
                continue;

            if (!_nodeManager.TryGetComponent<DamageDealtMultiplierComponent>(ability,
                    out var damageDealtMultiplierComponent))
                continue;
            
            if (damageDealtMultiplierComponent.DamageDealtTypeMultiplier.TryGetValue(
                    damageType, out var typeMultiplier))
                multiplier *= typeMultiplier;
        }

        // Check a mob's status effects for any kind of multiplicative damage increase
        foreach (var statusEffect in node.Comp.StatusEffects.Values)
        {
            if (!_nodeManager.TryGetComponent<DamageDealtMultiplierComponent>(statusEffect,
                    out var damageDealtMultiplierComponent))
                continue;
            
            if (damageDealtMultiplierComponent.DamageDealtTypeMultiplier.TryGetValue(
                    damageType, out var typeMultiplier))
                multiplier *= typeMultiplier;
        }
        
        return multiplier;
    }

    public float GetFlatDamageIncrease(Node<MobComponent> node, DamageCategory damageCategory, DamageType damageType)
    {
        var damageIncrease = 0f;
        
        // Check a mob's abilities for any kind of flat damage increase
        foreach (var abilityName in node.Comp.Abilities)
        {
            if (!_abilitySystem.TryGetAbility(abilityName, out var ability))
                continue;

            // Increase damage by flat amount
            if (!_nodeManager.TryGetComponent<FlatDamageIncreaseComponent>(ability,
                    out var flatDamageIncreaseComponent))
                continue;
            
            if (flatDamageIncreaseComponent.FlatDamageCategoryIncrease.TryGetValue(
                    damageCategory, out var categoryFlat))
                damageIncrease += categoryFlat;
            
            if (flatDamageIncreaseComponent.FlatDamageTypeIncrease.TryGetValue(
                    damageType, out var typeFlat))
                damageIncrease += typeFlat;
        }
        
        // Check a mob's status effects for any kind of flat damage increase
        foreach (var statusEffect in node.Comp.StatusEffects.Values)
        {
            // Increase damage by flat amount
            if (!_nodeManager.TryGetComponent<FlatDamageIncreaseComponent>(statusEffect,
                    out var flatDamageIncreaseComponent))
                continue;
            
            if (flatDamageIncreaseComponent.FlatDamageCategoryIncrease.TryGetValue(
                    damageCategory, out var categoryFlat))
                damageIncrease += categoryFlat;
            
            if (flatDamageIncreaseComponent.FlatDamageTypeIncrease.TryGetValue(
                    damageType, out var typeFlat))
                damageIncrease += typeFlat;
        }

        return damageIncrease;
    }
    
    public float GetFlatDamageIncrease(Node<MobComponent> node, DamageCategory damageCategory)
    {
        var damageIncrease = 0f;
        
        // Check a mob's abilities for any kind of flat damage increase
        foreach (var abilityName in node.Comp.Abilities)
        {
            if (!_abilitySystem.TryGetAbility(abilityName, out var ability))
                continue;

            // Increase damage by flat amount
            if (!_nodeManager.TryGetComponent<FlatDamageIncreaseComponent>(ability,
                    out var flatDamageIncreaseComponent))
                continue;
            
            if (flatDamageIncreaseComponent.FlatDamageCategoryIncrease.TryGetValue(
                    damageCategory, out var categoryFlat))
                damageIncrease += categoryFlat;
        }
        
        // Check a mob's status effects for any kind of flat damage increase
        foreach (var statusEffect in node.Comp.StatusEffects.Values)
        {
            // Increase damage by flat amount
            if (!_nodeManager.TryGetComponent<FlatDamageIncreaseComponent>(statusEffect,
                    out var flatDamageIncreaseComponent))
                continue;
            
            if (flatDamageIncreaseComponent.FlatDamageCategoryIncrease.TryGetValue(
                    damageCategory, out var categoryFlat))
                damageIncrease += categoryFlat;
        }

        return damageIncrease;
    }
    
    public float GetFlatDamageIncrease(Node<MobComponent> node, DamageType damageType)
    {
        var damageIncrease = 0f;
        
        // Check a mob's abilities for any kind of flat damage increase
        foreach (var abilityName in node.Comp.Abilities)
        {
            if (!_abilitySystem.TryGetAbility(abilityName, out var ability))
                continue;

            // Increase damage by flat amount
            if (!_nodeManager.TryGetComponent<FlatDamageIncreaseComponent>(ability,
                    out var flatDamageIncreaseComponent))
                continue;
            
            if (flatDamageIncreaseComponent.FlatDamageTypeIncrease.TryGetValue(
                    damageType, out var typeFlat))
                damageIncrease += typeFlat;
        }
        
        // Check a mob's status effects for any kind of flat damage increase
        foreach (var statusEffect in node.Comp.StatusEffects.Values)
        {
            // Increase damage by flat amount
            if (!_nodeManager.TryGetComponent<FlatDamageIncreaseComponent>(statusEffect,
                    out var flatDamageIncreaseComponent))
                continue;
            
            if (flatDamageIncreaseComponent.FlatDamageTypeIncrease.TryGetValue(
                    damageType, out var typeFlat))
                damageIncrease += typeFlat;
        }

        return damageIncrease;
    }
}