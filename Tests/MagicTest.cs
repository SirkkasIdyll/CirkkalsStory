using GdUnit4;
using Godot;
using PC.Components.Magic;
using PC.Components.Mob;
using PC.SlimeFactory;
using static GdUnit4.Assertions;
namespace PC.Tests;

[TestSuite][RequireGodotRuntime]
public class MagicTest
{
    private readonly Node _testScene = new();
    
    private readonly NodeSystemManager _nodeSystemManager = NodeSystemManager.Instance;
    private readonly NodeManager _nodeManager = NodeManager.Instance;
    private MagicSystem _magicSystem = null!;
    
    [Before]
    public void Before()
    {
        AddNode(_testScene);
        _nodeSystemManager.InitializeNodeSystems(_testScene);
        _nodeSystemManager.InjectNodeSystemDependencies();
        
        if (_nodeSystemManager.TryGetNodeSystem<MagicSystem>(out var magicSystem))
            _magicSystem = magicSystem;
    }

    /// <summary>
    /// Check that we're even able to get the NodeSystem
    /// </summary>
    [TestCase]
    public void MagicSystemExists()
    {
        AssertObject(_magicSystem).IsNotNull();
    }
    
    /// <summary>
    /// Check that all spells in MobComponents are valid
    /// </summary>
    [TestCase]
    public void ValidateMobSpells()
    {
        foreach (var node in _nodeManager.NodeDictionary.Values)
        {
            if (!_nodeManager.TryGetComponent<MobComponent>(node, out var mobComponent))
                continue;

            foreach (var spellName in mobComponent.Spells)
            {
                AssertBool(_magicSystem.SpellExists(spellName))
                    .OverrideFailureMessage(node.Name + " has invalid spell called " + spellName).IsTrue();
            }
        }
    }

    [TestCase]
    public void TestManaUsage()
    {
        var spell = new Node();
        AssertBool(_nodeManager.TryAddComponent<SpellComponent>(spell)).IsTrue();
        AssertBool(_nodeManager.TryGetComponent<SpellComponent>(spell, out var spellComponent)).IsTrue();
        AssertBool(_nodeManager.TryAddComponent<ManaCostComponent>(spell)).IsTrue();
        AssertBool(_nodeManager.TryGetComponent<ManaCostComponent>(spell, out var manaCostComponent)).IsTrue();
        manaCostComponent!.ManaCost = 5;
        AddNode(spell);
        AutoFree(spellComponent);
        AutoFree(manaCostComponent);
        
        var spellcaster = new Node();
        AssertBool(_nodeManager.TryAddComponent<MobComponent>(spellcaster)).IsTrue();
        AssertBool(_nodeManager.TryGetComponent<MobComponent>(spellcaster, out var mobComponent)).IsTrue();
        AssertBool(_nodeManager.TryAddComponent<ManaComponent>(spellcaster)).IsTrue();
        AssertBool(_nodeManager.TryGetComponent<ManaComponent>(spellcaster, out var manaComponent)).IsTrue();
        manaComponent!.MaxMana = 100;
        manaComponent!.Mana = 100;
        AddNode(spellcaster);
        AutoFree(mobComponent);
        AutoFree(manaComponent);
        
        _magicSystem.CastSpell((spellcaster, mobComponent!), (spell, spellComponent!), false);
        AssertInt(manaComponent.Mana).IsEqual(95);
    }
}