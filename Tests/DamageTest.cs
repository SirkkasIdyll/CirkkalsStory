using CS.Components.Ability;
using CS.Components.Damage;
using CS.Components.Damageable;
using CS.Components.Mob;
using CS.SlimeFactory;
using GdUnit4;
using Godot;
using static GdUnit4.Assertions;

namespace CS.Tests;

[TestSuite][RequireGodotRuntime]
public class DamageTest
{
    private readonly Node _testScene = new();
    
    private readonly NodeSystemManager _nodeSystemManager = NodeSystemManager.Instance;
    private readonly NodeManager _nodeManager = NodeManager.Instance;
    private DamageSystem _damageSystem = null!;
    private DamageableSystem _damageableSystem = null!;
    
    [Before]
    public void Before()
    {
        AddNode(_testScene);
        _nodeSystemManager.InitializeNodeSystems(_testScene);
        _nodeSystemManager.InjectNodeSystemDependencies();
        
        if (_nodeSystemManager.TryGetNodeSystem<DamageSystem>(out var damageSystem))
            _damageSystem = damageSystem;
        
        if (_nodeSystemManager.TryGetNodeSystem<DamageableSystem>(out var damageableSystem))
            _damageableSystem = damageableSystem;
    }
    
    /// <summary>
    /// Check that we're even able to get the NodeSystem
    /// </summary>
    [TestCase]
    public void DamageSystemExists()
    {
        AssertObject(_damageSystem).IsNotNull();
        AssertObject(_damageableSystem).IsNotNull();
    }

    /// <summary>
    /// Attack an enemy, enemy should take expected damage
    /// </summary>
    [TestCase]
    public void TestDamageTaken()
    {
        var attacker = new Node();
        AddNode(attacker);
        
        var attack = new Node();
        AssertBool(_nodeManager.TryAddComponent<DamageComponent>(attack)).IsTrue();
        AssertBool(_nodeManager.TryGetComponent<DamageComponent>(attack, out var damageComponent)).IsTrue();
        damageComponent!.Damage = 5;
        AddNode(attack);
        AutoFree(damageComponent);
        
        var defender = new Node();
        AssertBool(_nodeManager.TryAddComponent<DamageableComponent>(defender)).IsTrue();
        AssertBool(_nodeManager.TryGetComponent<DamageableComponent>(defender, out var damageableComponent)).IsTrue();
        AssertBool(_nodeManager.TryAddComponent<HealthComponent>(defender)).IsTrue();
        AssertBool(_nodeManager.TryGetComponent<HealthComponent>(defender, out var healthComponent)).IsTrue();
        healthComponent!.MaxHealth = 100;
        healthComponent!.Health = 100;
        AddNode(defender);
        AutoFree(damageableComponent);
        AutoFree(healthComponent);
        
        _damageSystem.TryDamageTarget((attack, damageComponent), defender, attacker, out var damageDealt);
        AssertInt(damageDealt).IsEqual(5);
        AssertInt(healthComponent.Health).IsEqual(95);
    }
    
    /// <summary>
    /// Attack an enemy, enemy should take expected damage
    /// </summary>
    [TestCase]
    public void TestDamageIncreasingAbilities()
    {
        var attacker = new Node();
        AssertBool(_nodeManager.TryAddComponent<MobComponent>(attacker)).IsTrue();
        AssertBool(_nodeManager.TryGetComponent<MobComponent>(attacker, out var mobComponent)).IsTrue();
        AddNode(attacker);
        AutoFree(mobComponent);

        _nodeSystemManager.TryGetNodeSystem<AbilitySystem>(out var abilitySystem);
        
        var damageDealtMultiplierAbility = new Node();
        AssertBool(_nodeManager.TryAddComponent<AbilityComponent>(damageDealtMultiplierAbility)).IsTrue();
        AssertBool(_nodeManager.TryGetComponent<AbilityComponent>(damageDealtMultiplierAbility, out var multAbilityComponent)).IsTrue();
        AssertBool(_nodeManager.TryAddComponent<DamageDealtMultiplierComponent>(damageDealtMultiplierAbility)).IsTrue();
        AssertBool(_nodeManager.TryGetComponent<DamageDealtMultiplierComponent>(damageDealtMultiplierAbility, out var damageDealtMultiplierComponent)).IsTrue();
        damageDealtMultiplierComponent!.DamageDealtCategoryMultiplier.Add(DamageCategory.Physical, 2f);
        abilitySystem!.AbilityDictionary.Add("damageDealtMultiplier", damageDealtMultiplierAbility);
        mobComponent!.Abilities.Add("damageDealtMultiplier");
        AutoFree(damageDealtMultiplierAbility);
        AutoFree(multAbilityComponent);
        AutoFree(damageDealtMultiplierComponent);
        
        var flatDamageIncreaseAbility = new Node();
        AssertBool(_nodeManager.TryAddComponent<AbilityComponent>(flatDamageIncreaseAbility)).IsTrue();
        AssertBool(_nodeManager.TryGetComponent<AbilityComponent>(flatDamageIncreaseAbility, out var flatAbilityComponent)).IsTrue();
        AssertBool(_nodeManager.TryAddComponent<FlatDamageIncreaseComponent>(flatDamageIncreaseAbility)).IsTrue();
        AssertBool(_nodeManager.TryGetComponent<FlatDamageIncreaseComponent>(flatDamageIncreaseAbility, out var flatDamageIncreaseComponent)).IsTrue();
        flatDamageIncreaseComponent!.FlatDamageTypeIncrease.Add(DamageType.Blunt, 3f);
        abilitySystem!.AbilityDictionary.Add("flatDamageIncrease", flatDamageIncreaseAbility);
        mobComponent!.Abilities.Add("flatDamageIncrease");
        AutoFree(flatDamageIncreaseAbility);
        AutoFree(flatAbilityComponent);
        AutoFree(flatDamageIncreaseComponent);
        
        var attack = new Node();
        AssertBool(_nodeManager.TryAddComponent<DamageComponent>(attack)).IsTrue();
        AssertBool(_nodeManager.TryGetComponent<DamageComponent>(attack, out var damageComponent)).IsTrue();
        damageComponent!.Damage = 5;
        damageComponent.DamageCategory = DamageCategory.Physical;
        damageComponent.DamageType = DamageType.Blunt;
        AddNode(attack);
        AutoFree(damageComponent);
        
        var defender = new Node();
        AssertBool(_nodeManager.TryAddComponent<DamageableComponent>(defender)).IsTrue();
        AssertBool(_nodeManager.TryGetComponent<DamageableComponent>(defender, out var damageableComponent)).IsTrue();
        AssertBool(_nodeManager.TryAddComponent<HealthComponent>(defender)).IsTrue();
        AssertBool(_nodeManager.TryGetComponent<HealthComponent>(defender, out var healthComponent)).IsTrue();
        healthComponent!.MaxHealth = 100;
        healthComponent!.Health = 100;
        AddNode(defender);
        AutoFree(damageableComponent);
        AutoFree(healthComponent);
        
        _damageSystem.TryDamageTarget((attack, damageComponent), defender, attacker, out var damageDealt);
        AssertInt(damageDealt).IsEqual(13);
        AssertInt(healthComponent.Health).IsEqual(87);
    }
    
        /// <summary>
    /// Attack an enemy, enemy should take expected damage
    /// </summary>
    [TestCase]
    public void TestDamageResistingAbilities()
    {
        var attacker = new Node();
        AddNode(attacker);

        _nodeSystemManager.TryGetNodeSystem<AbilitySystem>(out var abilitySystem);
        
        var attack = new Node();
        AssertBool(_nodeManager.TryAddComponent<DamageComponent>(attack)).IsTrue();
        AssertBool(_nodeManager.TryGetComponent<DamageComponent>(attack, out var damageComponent)).IsTrue();
        damageComponent!.Damage = 20;
        damageComponent.DamageCategory = DamageCategory.Physical;
        damageComponent.DamageType = DamageType.Blunt;
        AddNode(attack);
        AutoFree(damageComponent);
        
        var defender = new Node();
        AssertBool(_nodeManager.TryAddComponent<MobComponent>(defender)).IsTrue();
        AssertBool(_nodeManager.TryGetComponent<MobComponent>(defender, out var mobComponent)).IsTrue();
        AssertBool(_nodeManager.TryAddComponent<DamageableComponent>(defender)).IsTrue();
        AssertBool(_nodeManager.TryGetComponent<DamageableComponent>(defender, out var damageableComponent)).IsTrue();
        AssertBool(_nodeManager.TryAddComponent<HealthComponent>(defender)).IsTrue();
        AssertBool(_nodeManager.TryGetComponent<HealthComponent>(defender, out var healthComponent)).IsTrue();
        healthComponent!.MaxHealth = 100;
        healthComponent!.Health = 100;
        AddNode(defender);
        AutoFree(mobComponent);
        AutoFree(damageableComponent);
        AutoFree(healthComponent);
        
        var damageResistMultiplierAbility = new Node();
        AssertBool(_nodeManager.TryAddComponent<AbilityComponent>(damageResistMultiplierAbility)).IsTrue();
        AssertBool(_nodeManager.TryGetComponent<AbilityComponent>(damageResistMultiplierAbility, out var multAbilityComponent)).IsTrue();
        AssertBool(_nodeManager.TryAddComponent<DamageResistMultiplierComponent>(damageResistMultiplierAbility)).IsTrue();
        AssertBool(_nodeManager.TryGetComponent<DamageResistMultiplierComponent>(damageResistMultiplierAbility, out var damageResistMultiplierComponent)).IsTrue();
        damageResistMultiplierComponent!.DamageCategoryResistanceMultiplier.Add(DamageCategory.Physical, 1.25f);
        abilitySystem!.AbilityDictionary.Add("damageResistMultiplier", damageResistMultiplierAbility);
        mobComponent!.Abilities.Add("damageResistMultiplier");
        AutoFree(damageResistMultiplierAbility);
        AutoFree(multAbilityComponent);
        AutoFree(damageResistMultiplierComponent);
        
        var flatDamageResistAbility = new Node();
        AssertBool(_nodeManager.TryAddComponent<AbilityComponent>(flatDamageResistAbility)).IsTrue();
        AssertBool(_nodeManager.TryGetComponent<AbilityComponent>(flatDamageResistAbility, out var flatAbilityComponent)).IsTrue();
        AssertBool(_nodeManager.TryAddComponent<FlatDamageResistComponent>(flatDamageResistAbility)).IsTrue();
        AssertBool(_nodeManager.TryGetComponent<FlatDamageResistComponent>(flatDamageResistAbility, out var flatDamageResistComponent)).IsTrue();
        flatDamageResistComponent!.FlatDamageTypeResistance.Add(DamageType.Blunt, 3f);
        abilitySystem!.AbilityDictionary.Add("flatDamageResist", flatDamageResistAbility);
        mobComponent!.Abilities.Add("flatDamageResist");
        AutoFree(flatDamageResistAbility);
        AutoFree(flatAbilityComponent);
        AutoFree(flatDamageResistComponent);
        
        _damageSystem.TryDamageTarget((attack, damageComponent), defender, attacker, out var damageDealt);
        AssertInt(damageDealt).IsEqual(13);
        AssertInt(healthComponent.Health).IsEqual(87);
    }
}