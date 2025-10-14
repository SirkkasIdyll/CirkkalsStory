using System.Threading.Tasks;
using CS.Components.Clothing;
using CS.Components.Inventory;
using CS.SlimeFactory;
using GdUnit4;
using static GdUnit4.Assertions;
using Godot;

namespace CS.Tests;

[TestSuite][RequireGodotRuntime]
public class InventoryTest
{
    private readonly Node _testScene = new();
    private const int DelayTime = 150;
    
    private readonly NodeSystemManager _nodeSystemManager = NodeSystemManager.Instance;
    private readonly NodeManager _nodeManager = NodeManager.Instance;
    private ClothingSystem _clothingSystem = null!;
    private StorageSystem _storageSystem = null!;
    
    [Before]
    public void Before()
    {
        AddNode(_testScene);
        _nodeSystemManager.InitializeNodeSystems(_testScene);
        _nodeSystemManager.InjectNodeSystemDependencies();
        
        if (_nodeSystemManager.TryGetNodeSystem<ClothingSystem>(out var clothingSystem))
            _clothingSystem = clothingSystem;
        
        if (_nodeSystemManager.TryGetNodeSystem<StorageSystem>(out var storageSystem))
            _storageSystem = storageSystem;
    }

    [TestCase]
    public void ClothingSystemExists()
    {
        AssertObject(_clothingSystem).IsNotNull();
    }

    [TestCase]
    public void StorageSystemExists()
    {
        AssertObject(_storageSystem).IsNotNull();
    }

    [TestCase]
    public async Task PickUpItem()
    {
        _nodeManager.TrySpawnNode("MobPlayer", out var mobPlayer);
        AssertObject(mobPlayer).IsNotNull();
        AddNode(mobPlayer!);

        _nodeManager.TrySpawnNode("RedHalfrimGlasses", out var glasses);
        AssertObject(glasses).IsNotNull();
        AddNode(glasses!);
        if (glasses is Node2D notEquippedYetNode2D)
            AssertBool(notEquippedYetNode2D.IsVisibleInTree()).IsTrue();
        
        _nodeManager.TryGetComponent<WearsClothingComponent>(mobPlayer!, out var wearsClothingComponent);
        AssertObject(wearsClothingComponent).IsNotNull();
        _nodeManager.TryGetComponent<StorableComponent>(glasses!, out var storableComponent);
        AssertObject(storableComponent).IsNotNull();

        // Wait through the pickup animation which currently only lasts 125 ms
        AssertBool(_clothingSystem.TryPutItemInHand((mobPlayer!, wearsClothingComponent!),
            (glasses!, storableComponent!))).IsTrue();
        await Task.Delay(DelayTime);
        
        AssertBool(wearsClothingComponent!.ClothingSlots[ClothingSlot.Inhand] == glasses).IsTrue();
        AssertBool(storableComponent!.StoredBy == mobPlayer!).IsTrue();
        if (glasses is Node2D currentlyEquippedNode2D)
            AssertBool(currentlyEquippedNode2D.IsVisibleInTree()).IsFalse();
    }

    [TestCase]
    public async Task DropItem()
    {
        _nodeManager.TrySpawnNode("MobPlayer", out var mobPlayer);
        AssertObject(mobPlayer).IsNotNull();
        AddNode(mobPlayer!);

        _nodeManager.TrySpawnNode("RedHalfrimGlasses", out var glasses);
        AssertObject(glasses).IsNotNull();
        AddNode(glasses!);
        
        _nodeManager.TryGetComponent<WearsClothingComponent>(mobPlayer!, out var wearsClothingComponent);
        AssertObject(wearsClothingComponent).IsNotNull();
        _nodeManager.TryGetComponent<StorableComponent>(glasses!, out var storableComponent);
        AssertObject(storableComponent).IsNotNull();
        
        // Wait through pickup animation
        AssertBool(_clothingSystem.TryPutItemInHand((mobPlayer!, wearsClothingComponent!),
            (glasses!, storableComponent!))).IsTrue();
        await Task.Delay(DelayTime);
        
        // Wait through drop movement, although mostly unnecessary
        AssertBool(_clothingSystem.TryDropitem((mobPlayer!, wearsClothingComponent!))).IsTrue();
        await Task.Delay(DelayTime);
        
        AssertBool(wearsClothingComponent!.ClothingSlots[ClothingSlot.Inhand] == null).IsTrue();
        AssertBool(storableComponent!.StoredBy == null).IsTrue();
        if (glasses is Node2D node2D)
            AssertBool(node2D.IsVisibleInTree()).IsTrue();
    }

    [TestCase]
    public async Task EquipClothing()
    {
        _nodeManager.TrySpawnNode("MobPlayer", out var mobPlayer);
        AssertObject(mobPlayer).IsNotNull();
        AddNode(mobPlayer!);

        _nodeManager.TrySpawnNode("RedHalfrimGlasses", out var glasses);
        AssertObject(glasses).IsNotNull();
        AddNode(glasses!);
        
        _nodeManager.TryGetComponent<WearsClothingComponent>(mobPlayer!, out var wearsClothingComponent);
        AssertObject(wearsClothingComponent).IsNotNull();
        _nodeManager.TryGetComponent<ClothingComponent>(glasses!, out var clothingComponent);
        AssertObject(clothingComponent).IsNotNull();
        
        AssertBool(_clothingSystem.TryEquipClothing((mobPlayer!, wearsClothingComponent!), (glasses!, clothingComponent!))).IsTrue();
        await Task.Delay(DelayTime);

        AssertBool(wearsClothingComponent!.ClothingSlots[clothingComponent!.ClothingSlot] == glasses).IsTrue();
        AssertBool(clothingComponent!.EquippedBy == mobPlayer).IsTrue();
        if (glasses is Node2D node2D)
            AssertBool(node2D.IsVisibleInTree()).IsFalse();

        var spriteSlot = mobPlayer!.GetNodeOrNull<AnimatedSprite2D>("CanvasGroup/" + clothingComponent!.ClothingSlot);
        AssertObject(spriteSlot).IsNotNull();
        AssertObject(spriteSlot.SpriteFrames).IsNotNull();
        AssertBool(spriteSlot.SpriteFrames == clothingComponent.EquippedSpriteFrames).IsTrue();
    }

    [TestCase]
    public async Task UnequipClothing()
    {
        _nodeManager.TrySpawnNode("MobPlayer", out var mobPlayer);
        AssertObject(mobPlayer).IsNotNull();
        AddNode(mobPlayer!);

        _nodeManager.TrySpawnNode("RedHalfrimGlasses", out var glasses);
        AssertObject(glasses).IsNotNull();
        AddNode(glasses!);
        
        _nodeManager.TryGetComponent<WearsClothingComponent>(mobPlayer!, out var wearsClothingComponent);
        AssertObject(wearsClothingComponent).IsNotNull();
        _nodeManager.TryGetComponent<ClothingComponent>(glasses!, out var clothingComponent);
        AssertObject(clothingComponent).IsNotNull();
        
        // Test out the unequip option without dropping the clothing item
        _clothingSystem.TryEquipClothing((mobPlayer!, wearsClothingComponent!), (glasses!, clothingComponent!));
        await Task.Delay(DelayTime);

        AssertBool(wearsClothingComponent!.ClothingSlots[clothingComponent!.ClothingSlot] == glasses).IsTrue();
        AssertBool(clothingComponent!.EquippedBy == mobPlayer).IsTrue();
        
        AssertBool(_clothingSystem.TryUnequipClothing((mobPlayer!, wearsClothingComponent!), clothingComponent.ClothingSlot)).IsTrue();
        await Task.Delay(DelayTime);
        
        AssertBool(wearsClothingComponent!.ClothingSlots[clothingComponent!.ClothingSlot] == null).IsTrue();
        AssertBool(clothingComponent!.EquippedBy == null).IsTrue();
        if (glasses is Node2D unequippedNode2D)
            AssertBool(unequippedNode2D.IsVisibleInTree()).IsFalse();
        
        // Test out the unequip drop option by re-equipping the clothing item
        _clothingSystem.TryEquipClothing((mobPlayer!, wearsClothingComponent!), (glasses!, clothingComponent!));
        await Task.Delay(DelayTime);
        
        AssertBool(wearsClothingComponent!.ClothingSlots[clothingComponent!.ClothingSlot] == glasses).IsTrue();
        AssertBool(clothingComponent!.EquippedBy == mobPlayer).IsTrue();

        AssertBool(_clothingSystem.TryUnequipClothing((mobPlayer!, wearsClothingComponent),
            clothingComponent.ClothingSlot, true));
        await Task.Delay(DelayTime);
        
        AssertBool(wearsClothingComponent!.ClothingSlots[clothingComponent!.ClothingSlot] == null).IsTrue();
        AssertBool(clothingComponent!.EquippedBy == null).IsTrue();
        if (glasses is Node2D droppedNode2D)
            AssertBool(droppedNode2D.IsVisibleInTree()).IsTrue();
    }

    [TestCase]
    public async Task EquipClothingFromHand()
    {
        _nodeManager.TrySpawnNode("MobPlayer", out var mobPlayer);
        AssertObject(mobPlayer).IsNotNull();
        AddNode(mobPlayer!);

        _nodeManager.TrySpawnNode("RedHalfrimGlasses", out var glasses);
        AssertObject(glasses).IsNotNull();
        AddNode(glasses!);
        
        _nodeManager.TryGetComponent<WearsClothingComponent>(mobPlayer!, out var wearsClothingComponent);
        AssertObject(wearsClothingComponent).IsNotNull();
        _nodeManager.TryGetComponent<StorableComponent>(glasses!, out var storableComponent);
        AssertObject(storableComponent).IsNotNull();
        _nodeManager.TryGetComponent<ClothingComponent>(glasses!, out var clothingComponent);
        AssertObject(clothingComponent).IsNotNull();
        
        // Wait through pickup animation
        AssertBool(_clothingSystem.TryPutItemInHand((mobPlayer!, wearsClothingComponent!),
            (glasses!, storableComponent!))).IsTrue();
        await Task.Delay(DelayTime);

        AssertBool(storableComponent!.StoredBy == mobPlayer).IsTrue();
        AssertBool(clothingComponent!.EquippedBy == null).IsTrue();
        AssertBool(wearsClothingComponent!.ClothingSlots[ClothingSlot.Inhand] == glasses).IsTrue();
        AssertBool(wearsClothingComponent!.ClothingSlots[clothingComponent.ClothingSlot] == null).IsTrue();
        
        AssertBool(_clothingSystem.TryEquipClothing((mobPlayer!, wearsClothingComponent!), (glasses!, clothingComponent!))).IsTrue();
        await Task.Delay(DelayTime);

        AssertBool(storableComponent!.StoredBy == null).IsTrue();
        AssertBool(clothingComponent!.EquippedBy == mobPlayer).IsTrue();
        AssertBool(wearsClothingComponent!.ClothingSlots[ClothingSlot.Inhand] == null).IsTrue();
        AssertBool(wearsClothingComponent!.ClothingSlots[clothingComponent.ClothingSlot] == glasses).IsTrue();
    }
}