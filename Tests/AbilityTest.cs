using GdUnit4;
using Godot;
using PC.Components.Ability;
using PC.Components.Mob;
using PC.SlimeFactory;
using static GdUnit4.Assertions;
namespace PC.Tests;

[TestSuite][RequireGodotRuntime]
public class AbilityTest
{
    private readonly Node _testScene = new();
    
    private readonly NodeSystemManager _nodeSystemManager = NodeSystemManager.Instance;
    private readonly NodeManager _nodeManager = NodeManager.Instance;
    private AbilitySystem _abilitySystem = null!;
    
    [Before]
    public void Before()
    {
        AddNode(_testScene);
        _nodeSystemManager.InitializeNodeSystems(_testScene);
        _nodeSystemManager.InjectNodeSystemDependencies();
        
        if (_nodeSystemManager.TryGetNodeSystem<AbilitySystem>(out var abilitySystem))
            _abilitySystem = abilitySystem;
    }
    
    /// <summary>
    /// Check that we're even able to get the NodeSystem
    /// </summary>
    [TestCase]
    public void AbilitySystemExists()
    {
        AssertObject(_abilitySystem).IsNotNull();
    }
    
    /// <summary>
    /// Check that all abilities in MobComponents are valid
    /// </summary>
    [TestCase]
    public void ValidateMobAbilities()
    {
        foreach (var node in _nodeManager.NodeDictionary.Values)
        {
            if (!_nodeManager.TryGetComponent<MobComponent>(node, out var mobComponent))
                continue;

            foreach (var abilityName in mobComponent.Abilities)
            {
                AssertBool(_abilitySystem.AbilityExists(abilityName))
                    .OverrideFailureMessage(node.Name + " has invalid ability called " + abilityName).IsTrue();
            }
        }
    }
}