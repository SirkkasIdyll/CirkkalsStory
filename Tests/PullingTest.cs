using System;
using System.Threading.Tasks;
using CS.Components.Grid;
using CS.Components.Pulling;
using CS.SlimeFactory;
using GdUnit4;
using static GdUnit4.Assertions;
using Godot;

namespace CS.Tests;

[TestSuite][RequireGodotRuntime]
public class PullingTest
{
    private readonly Node _testScene = new();
    private const int DelayTime = 150;
    private const float Tolerance = 0.01f;
    
    private readonly NodeSystemManager _nodeSystemManager = NodeSystemManager.Instance;
    private readonly NodeManager _nodeManager = NodeManager.Instance;
    private PullingSystem _pullingSystem = null!;
    private GridSystem _gridSystem = null!;
    
    [Before]
    public void Before()
    {
        AddNode(_testScene);
        _nodeSystemManager.InitializeNodeSystems(_testScene);
        _nodeSystemManager.InjectNodeSystemDependencies();

        if (_nodeSystemManager.TryGetNodeSystem<PullingSystem>(out var pullingSystem))
            _pullingSystem = pullingSystem;
        
        if (_nodeSystemManager.TryGetNodeSystem<GridSystem>(out var gridSystem))
            _gridSystem = gridSystem;
    }

    [TestCase]
    public void PullingSystemExists()
    {
        AssertObject(_pullingSystem).IsNotNull();
    }

    /// <summary>
    /// Try to pull a pair of red glasses
    /// </summary>
    [TestCase]
    public async Task StartPulling()
    {
        _nodeManager.TrySpawnNode("MobPlayer", out var mobPlayer);
        AssertObject(mobPlayer).IsNotNull();
        AddNode(mobPlayer!);
        
        _nodeManager.TrySpawnNode("RedHalfrimGlasses", out var glasses);
        AssertObject(glasses).IsNotNull();
        AddNode(glasses!);

        _nodeManager.TryGetComponent<CanPullThingsComponent>(mobPlayer!, out var canPullThingsComponent);
        AssertObject(canPullThingsComponent).IsNotNull();
        _nodeManager.TryGetComponent<PullableComponent>(glasses!, out var pullableComponent);
        AssertObject(pullableComponent).IsNotNull();

        AssertBool(_pullingSystem.TryPull((mobPlayer!, canPullThingsComponent!), (glasses!, pullableComponent!)))
            .IsTrue();
        await Task.Delay(DelayTime);

        AssertBool(canPullThingsComponent!.Target == glasses).IsTrue();
        AssertBool(pullableComponent!.PulledBy == mobPlayer).IsTrue();
        AssertBool(pullableComponent.IsBeingPulled).IsTrue();
    }

    [TestCase]
    public async Task PullingDistance()
    {
        _nodeManager.TrySpawnNode("MobPlayer", out var mobPlayer);
        AssertObject(mobPlayer).IsNotNull();
        AddNode(mobPlayer!);
        
        _nodeManager.TrySpawnNode("RedHalfrimGlasses", out var glasses);
        AssertObject(glasses).IsNotNull();
        AddNode(glasses!);

        _nodeManager.TryGetComponent<CanPullThingsComponent>(mobPlayer!, out var canPullThingsComponent);
        AssertObject(canPullThingsComponent).IsNotNull();
        _nodeManager.TryGetComponent<PullableComponent>(glasses!, out var pullableComponent);
        AssertObject(pullableComponent).IsNotNull();
        
        _gridSystem.SetPosition(glasses!, new Vector2(1, 1));
        
        AssertBool(_pullingSystem.TryPull((mobPlayer!, canPullThingsComponent!), (glasses!, pullableComponent!)))
            .IsTrue();
        await Task.Delay(DelayTime);

        AssertBool(canPullThingsComponent!.Target == glasses).IsTrue();
        AssertBool(pullableComponent!.PulledBy == mobPlayer).IsTrue();
        AssertBool(pullableComponent.IsBeingPulled).IsTrue();
        if (_gridSystem.TryGetDistance(mobPlayer!, glasses!, out var distance))
            AssertBool(Math.Abs(canPullThingsComponent.InitialPullDistance - distance.Value) < Tolerance).IsTrue();
    }

    /// <summary>
    /// Try to pull a pair of red glasses that are INSANELY out of range
    /// </summary>
    [TestCase]
    public async Task TryPullingOutOfRange()
    {
        _nodeManager.TrySpawnNode("MobPlayer", out var mobPlayer);
        AssertObject(mobPlayer).IsNotNull();
        AddNode(mobPlayer!);
        
        _nodeManager.TrySpawnNode("RedHalfrimGlasses", out var glasses);
        AssertObject(glasses).IsNotNull();
        AddNode(glasses!);

        _nodeManager.TryGetComponent<CanPullThingsComponent>(mobPlayer!, out var canPullThingsComponent);
        AssertObject(canPullThingsComponent).IsNotNull();
        _nodeManager.TryGetComponent<PullableComponent>(glasses!, out var pullableComponent);
        AssertObject(pullableComponent).IsNotNull();

        // Set position to some ridiculous value
        _gridSystem.SetPosition(glasses!, new Vector2(10000000f, 1000000f));
        
        AssertBool(_pullingSystem.TryPull((mobPlayer!, canPullThingsComponent!), (glasses!, pullableComponent!)))
            .IsFalse();
        await Task.Delay(DelayTime);
        
        AssertBool(canPullThingsComponent!.Target == null).IsTrue();
        AssertBool(pullableComponent!.PulledBy == null).IsTrue();
        AssertBool(pullableComponent.IsBeingPulled).IsFalse();
    }

    /// <summary>
    /// Try to pull a pair of red glasses, then try to pull a satchel after that
    /// </summary>
    [TestCase]
    public async Task TryPullingDifferentTarget()
    {
        _nodeManager.TrySpawnNode("MobPlayer", out var mobPlayer);
        AssertObject(mobPlayer).IsNotNull();
        AddNode(mobPlayer!);
        
        _nodeManager.TrySpawnNode("RedHalfrimGlasses", out var glasses);
        AssertObject(glasses).IsNotNull();
        AddNode(glasses!);
        
        _nodeManager.TrySpawnNode("Satchel", out var satchel);
        AssertObject(satchel).IsNotNull();
        AddNode(satchel!);
        
        _nodeManager.TryGetComponent<CanPullThingsComponent>(mobPlayer!, out var canPullThingsComponent);
        AssertObject(canPullThingsComponent).IsNotNull();
        _nodeManager.TryGetComponent<PullableComponent>(glasses!, out var pullableComponent);
        AssertObject(pullableComponent).IsNotNull();
        _nodeManager.TryGetComponent<PullableComponent>(satchel!, out var twoPullableComponent);
        AssertObject(twoPullableComponent).IsNotNull();
        
        AssertBool(_pullingSystem.TryPull((mobPlayer!, canPullThingsComponent!), (glasses!, pullableComponent!)))
            .IsTrue();
        await Task.Delay(DelayTime);
        
        AssertBool(canPullThingsComponent!.Target == glasses).IsTrue();
        AssertBool(pullableComponent!.PulledBy == mobPlayer).IsTrue();
        AssertBool(pullableComponent.IsBeingPulled).IsTrue();
        
        AssertBool(_pullingSystem.TryPull((mobPlayer!, canPullThingsComponent!), (satchel!, twoPullableComponent!)))
            .IsTrue();
        await Task.Delay(DelayTime);
        
        AssertBool(canPullThingsComponent!.Target == satchel).IsTrue();
        AssertBool(pullableComponent!.PulledBy == null).IsTrue();
        AssertBool(pullableComponent.IsBeingPulled).IsFalse();
        AssertBool(twoPullableComponent!.PulledBy == mobPlayer).IsTrue();
        AssertBool(twoPullableComponent.IsBeingPulled).IsTrue();
    }

    /// <summary>
    /// Try to pull a pair of red glasses, then stop pulling them
    /// </summary>
    [TestCase]
    public async Task StopPulling()
    {
        _nodeManager.TrySpawnNode("MobPlayer", out var mobPlayer);
        AssertObject(mobPlayer).IsNotNull();
        AddNode(mobPlayer!);
        
        _nodeManager.TrySpawnNode("RedHalfrimGlasses", out var glasses);
        AssertObject(glasses).IsNotNull();
        AddNode(glasses!);

        _nodeManager.TryGetComponent<CanPullThingsComponent>(mobPlayer!, out var canPullThingsComponent);
        AssertObject(canPullThingsComponent).IsNotNull();
        _nodeManager.TryGetComponent<PullableComponent>(glasses!, out var pullableComponent);
        AssertObject(pullableComponent).IsNotNull();
        
        AssertBool(_pullingSystem.TryPull((mobPlayer!, canPullThingsComponent!), (glasses!, pullableComponent!)))
            .IsTrue();
        await Task.Delay(DelayTime);

        AssertBool(canPullThingsComponent!.Target == glasses).IsTrue();
        AssertBool(pullableComponent!.PulledBy == mobPlayer).IsTrue();
        AssertBool(pullableComponent.IsBeingPulled).IsTrue();
        
        AssertBool(_pullingSystem.TryStopPull((mobPlayer!, canPullThingsComponent!), (glasses!, pullableComponent!)))
            .IsTrue();
        await Task.Delay(DelayTime);
        
        AssertBool(canPullThingsComponent!.Target == null).IsTrue();
        AssertBool(pullableComponent!.PulledBy == null).IsTrue();
        AssertBool(pullableComponent.IsBeingPulled).IsFalse();
    }
}