using System;
using System.Threading.Tasks;
using GdUnit4;
using static GdUnit4.Assertions;
using Godot;
using PC.Components.Clothing;
using PC.Components.Inventory;
using PC.SlimeFactory;

namespace PC.Tests;

[TestSuite][RequireGodotRuntime]
public class InventoryTest
{
    private readonly Node _testScene = new();
    private const int DelayTime = 150;
    private const float Tolerance = 0.01f;
    
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

    /// <summary>
    /// Put a pair of glasses in the player's hands
    /// </summary>
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

    /// <summary>
    /// Put a pair of glasses in the player's hands then drop it
    /// </summary>
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

    /// <summary>
    /// Equip a pair of glasses to the player
    /// </summary>
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
    public async Task EquipClothingOverClothing()
    {
        _nodeManager.TrySpawnNode("MobPlayer", out var mobPlayer);
        AssertObject(mobPlayer).IsNotNull();
        AddNode(mobPlayer!);

        _nodeManager.TrySpawnNode("RedHalfrimGlasses", out var glasses);
        AssertObject(glasses).IsNotNull();
        AddNode(glasses!);
        
        _nodeManager.TrySpawnNode("RedHalfrimGlasses", out var glassesTwo);
        AssertObject(glassesTwo).IsNotNull();
        AddNode(glassesTwo!);
        
        _nodeManager.TryGetComponent<WearsClothingComponent>(mobPlayer!, out var wearsClothingComponent);
        AssertObject(wearsClothingComponent).IsNotNull();
        _nodeManager.TryGetComponent<ClothingComponent>(glasses!, out var clothingComponent);
        AssertObject(clothingComponent).IsNotNull();
        _nodeManager.TryGetComponent<ClothingComponent>(glassesTwo!, out var gClothingComponent);
        AssertObject(gClothingComponent).IsNotNull();
        
        AssertBool(_clothingSystem.TryEquipClothing((mobPlayer!, wearsClothingComponent!), (glasses!, clothingComponent!))).IsTrue();
        await Task.Delay(DelayTime);

        AssertBool(wearsClothingComponent!.ClothingSlots[clothingComponent!.ClothingSlot] == glasses).IsTrue();
        AssertBool(clothingComponent!.EquippedBy == mobPlayer).IsTrue();
        
        AssertBool(_clothingSystem.TryEquipClothing((mobPlayer!, wearsClothingComponent!), (glassesTwo!, gClothingComponent!))).IsFalse();
        await Task.Delay(DelayTime);

        AssertBool(wearsClothingComponent!.ClothingSlots[clothingComponent!.ClothingSlot] == glasses).IsTrue();
        AssertBool(clothingComponent!.EquippedBy == mobPlayer).IsTrue();
        AssertBool(wearsClothingComponent!.ClothingSlots[gClothingComponent!.ClothingSlot] == glassesTwo).IsFalse();
        AssertBool(gClothingComponent!.EquippedBy == mobPlayer).IsFalse();
        
        AssertBool(_clothingSystem.TryEquipClothing((mobPlayer!, wearsClothingComponent!), (glassesTwo!, gClothingComponent!), true)).IsTrue();
        await Task.Delay(DelayTime);
        
        AssertBool(wearsClothingComponent!.ClothingSlots[clothingComponent!.ClothingSlot] == glasses).IsFalse();
        AssertBool(clothingComponent!.EquippedBy == mobPlayer).IsFalse();
        AssertBool(wearsClothingComponent!.ClothingSlots[gClothingComponent!.ClothingSlot] == glassesTwo).IsTrue();
        AssertBool(gClothingComponent!.EquippedBy == mobPlayer).IsTrue();
    }

    /// <summary>
    /// Equip a pair of glasses to the player, then unequip it, then equip it, then unequip it with the drop option set
    /// </summary>
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

    /// <summary>
    /// Put a pair of glasses in the player's hands, then equip the glasses to the player's face
    /// </summary>
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

    [TestCase]
    public async Task PutEquippedClothingInHand()
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
        
        AssertBool(_clothingSystem.TryEquipClothing((mobPlayer!, wearsClothingComponent!), (glasses!, clothingComponent!))).IsTrue();
        await Task.Delay(DelayTime);
        
        AssertBool(clothingComponent!.EquippedBy == mobPlayer).IsTrue();
        AssertBool(wearsClothingComponent!.ClothingSlots[clothingComponent.ClothingSlot] == glasses).IsTrue();
        AssertBool(storableComponent!.StoredBy == null).IsTrue();

        AssertBool(_clothingSystem.TryPutItemInHand((mobPlayer!, wearsClothingComponent!),
            (glasses!, storableComponent!))).IsTrue();
        await Task.Delay(DelayTime);
        
        AssertBool(storableComponent!.StoredBy == mobPlayer).IsTrue();
        AssertBool(clothingComponent!.EquippedBy == null).IsTrue();
        AssertBool(wearsClothingComponent!.ClothingSlots[ClothingSlot.Inhand] == glasses).IsTrue();
        AssertBool(wearsClothingComponent!.ClothingSlots[clothingComponent.ClothingSlot] == null).IsTrue();
        
    }

    /// <summary>
    /// Put a pair of glasses into a satchel
    /// </summary>
    [TestCase]
    public async Task PutItemInStorage()
    {
        _nodeManager.TrySpawnNode("RedHalfrimGlasses", out var glasses);
        AssertObject(glasses).IsNotNull();
        AddNode(glasses!);

        _nodeManager.TrySpawnNode("Satchel", out var satchel);
        AssertObject(satchel).IsNotNull();
        AddNode(satchel!);

        _nodeManager.TryGetComponent<StorageComponent>(satchel!, out var storageComponent);
        AssertObject(storageComponent).IsNotNull();
        _nodeManager.TryGetComponent<StorableComponent>(glasses!, out var storableComponent);
        AssertObject(storableComponent).IsNotNull();

        AssertBool(_storageSystem.TryAddItemToStorage((satchel!, storageComponent!), (glasses!, storableComponent!)));
        await Task.Delay(DelayTime);
        
        AssertBool(storageComponent!.VolumeOccupied > 0
                   && Math.Abs(storageComponent!.VolumeOccupied - storableComponent!.Volume) < Tolerance).IsTrue();
        AssertBool(storageComponent.Items[0] == glasses).IsTrue();
        AssertBool(storableComponent!.StoredBy == satchel).IsTrue();
        if (glasses is Node2D node2D)
            AssertBool(node2D.IsVisibleInTree()).IsFalse();
    }

    [TestCase]
    public async Task TransferItemBetweenStorages()
    {
        _nodeManager.TrySpawnNode("RedHalfrimGlasses", out var glasses);
        AssertObject(glasses).IsNotNull();
        AddNode(glasses!);

        _nodeManager.TrySpawnNode("Satchel", out var satchel);
        AssertObject(satchel).IsNotNull();
        AddNode(satchel!);
        
        _nodeManager.TrySpawnNode("Satchel", out var satchelTwo);
        AssertObject(satchelTwo).IsNotNull();
        AddNode(satchelTwo!);

        _nodeManager.TryGetComponent<StorageComponent>(satchel!, out var storageComponent);
        AssertObject(storageComponent).IsNotNull();
        _nodeManager.TryGetComponent<StorageComponent>(satchelTwo!, out var twoStorageComponent);
        AssertObject(twoStorageComponent).IsNotNull();
        _nodeManager.TryGetComponent<StorableComponent>(glasses!, out var storableComponent);
        AssertObject(storableComponent).IsNotNull();
        
        AssertBool(_storageSystem.TryAddItemToStorage((satchel!, storageComponent!), (glasses!, storableComponent!)));
        await Task.Delay(DelayTime);
        
        AssertBool(storageComponent!.VolumeOccupied > 0
                   && Math.Abs(storageComponent!.VolumeOccupied - storableComponent!.Volume) < Tolerance).IsTrue();
        AssertBool(storageComponent.Items[0] == glasses).IsTrue();
        AssertBool(storableComponent!.StoredBy == satchel).IsTrue();

        AssertBool(_storageSystem.TryAddItemToStorage((satchelTwo!, twoStorageComponent!), (glasses!, storableComponent!)))
            .IsTrue();
        await Task.Delay(DelayTime);
        
        AssertBool(storageComponent.Items.Count == 0).IsTrue();
        AssertBool(storageComponent.VolumeOccupied < Tolerance).IsTrue();
        AssertBool(twoStorageComponent!.VolumeOccupied > 0
                   && Math.Abs(twoStorageComponent!.VolumeOccupied - storableComponent!.Volume) < Tolerance).IsTrue();
        AssertBool(twoStorageComponent.Items[0] == glasses).IsTrue();
        AssertBool(storableComponent!.StoredBy == satchelTwo).IsTrue();
    }

    /// <summary>
    /// Put a pair of glasses into a satchel and then remove it from the satchel
    /// </summary>
    [TestCase]
    public async Task RemoveItemFromStorage()
    {
        _nodeManager.TrySpawnNode("RedHalfrimGlasses", out var glasses);
        AssertObject(glasses).IsNotNull();
        AddNode(glasses!);

        _nodeManager.TrySpawnNode("Satchel", out var satchel);
        AssertObject(satchel).IsNotNull();
        AddNode(satchel!);

        _nodeManager.TryGetComponent<StorageComponent>(satchel!, out var storageComponent);
        AssertObject(storageComponent).IsNotNull();
        _nodeManager.TryGetComponent<StorableComponent>(glasses!, out var storableComponent);
        AssertObject(storableComponent).IsNotNull();
        
        AssertBool(_storageSystem.TryAddItemToStorage((satchel!, storageComponent!), (glasses!, storableComponent!)));
        await Task.Delay(DelayTime);
        
        AssertBool(storageComponent!.Items[0] == glasses).IsTrue();
        AssertBool(storableComponent!.StoredBy == satchel).IsTrue();

        AssertBool(_storageSystem.TryRemoveItemFromStorage((satchel!, storageComponent!),
            (glasses!, storableComponent!), out var removedItem)).IsTrue();
        
        AssertObject(removedItem).IsNotNull();
        AssertBool(removedItem == glasses).IsTrue();
        AssertBool(storageComponent.Items.Count == 0).IsTrue();
        AssertBool(storableComponent.StoredBy == null).IsTrue();
        if (glasses is Node2D node2D)
            AssertBool(node2D.IsVisibleInTree()).IsFalse();
        AssertBool(storageComponent.VolumeOccupied < Tolerance).IsTrue();
    }

    /// <summary>
    /// Pick up a pair of glasses then put them into a satchel
    /// </summary>
    [TestCase]
    public async Task PutHeldItemInStorage()
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

        _nodeManager.TryGetComponent<WearsClothingComponent>(mobPlayer!, out var wearsClothingComponent);
        AssertObject(wearsClothingComponent).IsNotNull();
        _nodeManager.TryGetComponent<StorageComponent>(satchel!, out var storageComponent);
        AssertObject(storageComponent).IsNotNull();
        _nodeManager.TryGetComponent<StorableComponent>(glasses!, out var storableComponent);
        AssertObject(storableComponent).IsNotNull();
        
        // Wait through pickup animation
        AssertBool(_clothingSystem.TryPutItemInHand((mobPlayer!, wearsClothingComponent!),
            (glasses!, storableComponent!))).IsTrue();
        await Task.Delay(DelayTime);

        AssertBool(storableComponent!.StoredBy == mobPlayer).IsTrue();
        AssertBool(wearsClothingComponent!.ClothingSlots[ClothingSlot.Inhand] == glasses).IsTrue();

        // Transfer item to storage
        AssertBool(_storageSystem.TryAddItemToStorage((satchel!, storageComponent!), (glasses!, storableComponent!)))
            .IsTrue();
        await Task.Delay(DelayTime);

        AssertBool(storableComponent!.StoredBy == satchel).IsTrue();
        AssertBool(wearsClothingComponent!.ClothingSlots[ClothingSlot.Inhand] == null).IsTrue();
        AssertBool(storageComponent!.Items[0] == glasses).IsTrue();
    }

    /// <summary>
    /// Equip a pair of glasses, then put the equipped glasses into storage
    /// </summary>
    [TestCase]
    public async Task PutEquippedItemInStorage()
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

        _nodeManager.TryGetComponent<WearsClothingComponent>(mobPlayer!, out var wearsClothingComponent);
        AssertObject(wearsClothingComponent).IsNotNull();
        _nodeManager.TryGetComponent<StorageComponent>(satchel!, out var storageComponent);
        AssertObject(storageComponent).IsNotNull();
        _nodeManager.TryGetComponent<StorableComponent>(glasses!, out var storableComponent);
        AssertObject(storableComponent).IsNotNull();
        _nodeManager.TryGetComponent<ClothingComponent>(glasses!, out var clothingComponent);
        AssertObject(clothingComponent).IsNotNull();

        AssertBool(_clothingSystem.TryEquipClothing((mobPlayer!, wearsClothingComponent!),
            (glasses!, clothingComponent!))).IsTrue();
        await Task.Delay(DelayTime);
        
        AssertBool(wearsClothingComponent!.ClothingSlots[clothingComponent!.ClothingSlot] == glasses).IsTrue();
        AssertBool(clothingComponent!.EquippedBy == mobPlayer).IsTrue();
        AssertBool(storableComponent!.StoredBy == null).IsTrue();
        
        var spriteSlot = mobPlayer!.GetNodeOrNull<AnimatedSprite2D>("CanvasGroup/" + clothingComponent!.ClothingSlot);
        AssertObject(spriteSlot).IsNotNull();
        AssertObject(spriteSlot.SpriteFrames).IsNotNull();
        AssertBool(spriteSlot.SpriteFrames == clothingComponent.EquippedSpriteFrames).IsTrue();

        AssertBool(_storageSystem.TryAddItemToStorage((satchel!, storageComponent!), (glasses!, storableComponent!)))
            .IsTrue();
        await Task.Delay(DelayTime);

        AssertBool(wearsClothingComponent!.ClothingSlots[clothingComponent!.ClothingSlot] == null).IsTrue();
        AssertBool(clothingComponent.EquippedBy == null).IsTrue();
        AssertBool(storableComponent!.StoredBy == satchel).IsTrue();
        AssertObject(spriteSlot.SpriteFrames).IsNull();
    }

    /// <summary>
    /// Put a pair of glasses into a satchel, then put the glasses in hand
    /// </summary>
    [TestCase]
    public async Task PutStoredItemInHand()
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

        _nodeManager.TryGetComponent<WearsClothingComponent>(mobPlayer!, out var wearsClothingComponent);
        AssertObject(wearsClothingComponent).IsNotNull();
        _nodeManager.TryGetComponent<StorageComponent>(satchel!, out var storageComponent);
        AssertObject(storageComponent).IsNotNull();
        _nodeManager.TryGetComponent<StorableComponent>(glasses!, out var storableComponent);
        AssertObject(storableComponent).IsNotNull();

        AssertBool(_storageSystem.TryAddItemToStorage((satchel!, storageComponent!), (glasses!, storableComponent!)))
            .IsTrue();
        await Task.Delay(DelayTime);

        AssertBool(storageComponent!.Items[0] == glasses).IsTrue();
        AssertBool(storableComponent!.StoredBy == satchel).IsTrue();

        AssertBool(_clothingSystem.TryPutItemInHand((mobPlayer!, wearsClothingComponent!),
            (glasses!, storableComponent!))).IsTrue();
        await Task.Delay(DelayTime);

        AssertBool(storageComponent.Items.Count == 0).IsTrue();
        AssertBool(storableComponent.StoredBy == mobPlayer).IsTrue();
        AssertBool(wearsClothingComponent!.ClothingSlots[ClothingSlot.Inhand] == glasses).IsTrue();
    }

    /// <summary>
    /// Put a pair of glasses into a satchel, then equip the glasses directly
    /// </summary>
    [TestCase]
    public async Task EquipStoredItem()
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

        _nodeManager.TryGetComponent<WearsClothingComponent>(mobPlayer!, out var wearsClothingComponent);
        AssertObject(wearsClothingComponent).IsNotNull();
        _nodeManager.TryGetComponent<StorageComponent>(satchel!, out var storageComponent);
        AssertObject(storageComponent).IsNotNull();
        _nodeManager.TryGetComponent<StorableComponent>(glasses!, out var storableComponent);
        AssertObject(storableComponent).IsNotNull();
        _nodeManager.TryGetComponent<ClothingComponent>(glasses!, out var clothingComponent);
        AssertObject(clothingComponent).IsNotNull();

        AssertBool(_storageSystem.TryAddItemToStorage((satchel!, storageComponent!), (glasses!, storableComponent!)))
            .IsTrue();
        await Task.Delay(DelayTime);

        AssertBool(storageComponent!.Items[0] == glasses).IsTrue();
        AssertBool(storableComponent!.StoredBy == satchel).IsTrue();

        AssertBool(_clothingSystem.TryEquipClothing((mobPlayer!, wearsClothingComponent!),
            (glasses!, clothingComponent!))).IsTrue();
        await Task.Delay(DelayTime);
        
        AssertBool(storageComponent.Items.Count == 0).IsTrue();
        AssertBool(storableComponent.StoredBy == null).IsTrue();
        AssertBool(clothingComponent!.EquippedBy == mobPlayer).IsTrue();
        AssertBool(wearsClothingComponent!.ClothingSlots[clothingComponent!.ClothingSlot] == glasses).IsTrue();
        
        var spriteSlot = mobPlayer!.GetNodeOrNull<AnimatedSprite2D>("CanvasGroup/" + clothingComponent!.ClothingSlot);
        AssertObject(spriteSlot).IsNotNull();
        AssertObject(spriteSlot.SpriteFrames).IsNotNull();
        AssertBool(spriteSlot.SpriteFrames == clothingComponent.EquippedSpriteFrames).IsTrue();
    }
}