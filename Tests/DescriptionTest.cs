using GdUnit4;
using static GdUnit4.Assertions;
using Godot;
using PC.Components.Description;
using PC.SlimeFactory;

namespace PC.Tests;

[TestSuite][RequireGodotRuntime]
public class DescriptionTest
{
    private readonly Node _testScene = new();
    
    private readonly NodeSystemManager _nodeSystemManager = NodeSystemManager.Instance;
    private readonly NodeManager _nodeManager = NodeManager.Instance;
    private DescriptionSystem _descriptionSystem = null!;
    
    [Before]
    public void Before()
    {
        AddNode(_testScene);
        _nodeSystemManager.InitializeNodeSystems(_testScene);
        _nodeSystemManager.InjectNodeSystemDependencies();
        
        if (_nodeSystemManager.TryGetNodeSystem<DescriptionSystem>(out var descriptionSystem))
            _descriptionSystem = descriptionSystem;
    }
    
    /// <summary>
    /// Check that we're even able to get the NodeSystem
    /// </summary>
    [TestCase]
    public void DescriptionSystemExists()
    {
        AssertObject(_descriptionSystem).IsNotNull();
    }

    /// <summary>
    /// Everything that has a <see cref="DescriptionComponent"/> should also have the display name filled out
    /// </summary>
    [TestCase]
    public void ValidateDisplayNamesExist()
    {
        foreach (var node in _nodeManager.NodeDictionary.Values)
        {
            if (!_nodeManager.TryGetComponent<DescriptionComponent>(node, out var descriptionComponent))
                continue;
            
            if (!node.Name.ToString().StartsWith("Base"))
                AssertString(descriptionComponent.DisplayName)
                    .OverrideFailureMessage(node.Name + " does not have a display name set").IsNotEmpty();
        }
    }
}