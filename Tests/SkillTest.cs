using CS.Components.Mob;
using CS.Components.Skills;
using CS.SlimeFactory;
using GdUnit4;
using Godot;
using static GdUnit4.Assertions;
namespace CS.Tests;

[TestSuite][RequireGodotRuntime]
public class SkillTest
{
    private readonly Node _testScene = new();

    private readonly NodeSystemManager _nodeSystemManager = NodeSystemManager.Instance;
    private readonly NodeManager _nodeManager = NodeManager.Instance;
    private SkillSystem _skillSystem = null!;
    
    [Before]
    public void Before()
    {
        AddNode(_testScene);
        _nodeSystemManager.InitializeNodeSystems(_testScene);
        _nodeSystemManager.InjectNodeSystemDependencies();
        
        if (_nodeSystemManager.TryGetNodeSystem<SkillSystem>(out var skillSystem))
            _skillSystem = skillSystem;
    }

    /// <summary>
    /// Check that we're even able to get the NodeSystem
    /// </summary>
    [TestCase]
    public void SkillSystemExists()
    {
        AssertObject(_skillSystem).IsNotNull();
    }
    
    /// <summary>
    /// Check that all skills in MobComponents are valid
    /// </summary>
    [TestCase]
    public void ValidateMobSkills()
    {
        foreach (var node in _nodeManager.NodeDictionary.Values)
        {
            if (!_nodeManager.TryGetComponent<MobComponent>(node, out var mobComponent))
                continue;

            foreach (var skillName in mobComponent.Skills)
            {
                AssertBool(_skillSystem.SkillExists(skillName))
                    .OverrideFailureMessage(node.Name + " has invalid skill called " + skillName).IsTrue();
            }
        }
    }
}