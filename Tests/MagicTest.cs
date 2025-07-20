using CS.Components.Magic;
using CS.Components.Mob;
using CS.SlimeFactory;
using GdUnit4;
using Godot;
using static GdUnit4.Assertions;
namespace CS.Tests;

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
                AssertBool(_magicSystem.TryGetSpell(spellName, out var spell))
                    .OverrideFailureMessage(node.Name + " has invalid spell called " + spellName).IsTrue();
            }
        }
    }
}