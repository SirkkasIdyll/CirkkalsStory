using CS.Resources.Encounters;
using CS.SlimeFactory;
using GdUnit4;using static GdUnit4.Assertions;
using Godot;

namespace CS.Tests;

[TestSuite][RequireGodotRuntime]
public class ScenarioTest
{
    private readonly NodeManager _nodeManager = NodeManager.Instance;
    private const string EncounterFilePath = "res://Resources/Encounters/";

    [TestCase]
    public void ValidateCombatScenarios()
    {
        var files = _nodeManager.GetFilesByExtension(EncounterFilePath, ".tres");
        foreach (var file in files)
        {
            var combatScenarioResource = ResourceLoader.Load<CombatEncounterResource>(file);
            foreach (var mobName in combatScenarioResource.Mobs)
            {
                AssertBool(_nodeManager.NodeDictionary.TryGetValue(mobName, out var node))
                    .OverrideFailureMessage(file + " has invalid mob called " + mobName).IsTrue();
            }
        }
    }
}