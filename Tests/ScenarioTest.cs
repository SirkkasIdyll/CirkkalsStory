using CS.Resources.CombatScenarios;
using CS.SlimeFactory;
using GdUnit4;using static GdUnit4.Assertions;
using Godot;

namespace CS.Tests;

[TestSuite][RequireGodotRuntime]
public class ScenarioTest
{
    private readonly NodeManager _nodeManager = NodeManager.Instance;
    private const string CombatScenarioFilePath = "res://Resources/CombatScenarios/";

    [TestCase]
    public void ValidateCombatScenarios()
    {
        var files = _nodeManager.GetFilesByExtension(CombatScenarioFilePath, ".tres");
        foreach (var file in files)
        {
            var combatScenarioResource = ResourceLoader.Load<CombatScenarioResource>(file);
            foreach (var mobName in combatScenarioResource.Mobs)
            {
                AssertBool(_nodeManager.NodeDictionary.TryGetValue(mobName, out var node))
                    .OverrideFailureMessage(file + " has invalid mob called " + mobName).IsTrue();
            }
        }
    }
}