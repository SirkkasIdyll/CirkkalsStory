using CS.Nodes.UI.Audio;
using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Components.Inventory;

public partial class StorageComponent : Component
{
    /// <summary>
    /// The volume of the container that can be used to store <see cref="StorableComponent"/>
    /// </summary>
    [Export]
    public float Capacity;
    
    /// <summary>
    /// Sound played when closing the storage
    /// </summary>
    [Export]
    public AudioStreamWav? CloseSound;
    
    /// <summary>
    /// Sound played when inserting something into the storage
    /// </summary>
    [Export]
    public AudioStreamWav? InsertSound;
    
    /// <summary>
    /// What items are currently being stored
    /// </summary>
    [Export]
    public Array<Node> Items =  [];

    /// <summary>
    /// The largest possible item that can fit inside this storage
    /// </summary>
    [Export]
    public ItemSize MaxItemSize = ItemSize.Large;
    
    /// <summary>
    /// Sound played when opening the storage
    /// </summary>
    [Export]
    public AudioStreamWav? OpenSound;
    
    /// <summary>
    /// Sound played when removing something from storage
    /// </summary>
    [Export]
    public AudioStreamWav? RemoveSound;

    private float _volumeOccupied;

    /// <summary>
    /// How much volume is currently occupied by <see cref="StorableComponent"/>
    /// </summary>
    public float VolumeOccupied
    {
        get => _volumeOccupied;
        set => _volumeOccupied = float.Max(float.Min(value, Capacity), 0);
    }

    [ExportCategory("Instantiated")]
    [Export]
    public FluctuatingAudioStreamPlayer2DSystem? FluctuatingAudioStreamPlayer2DSystem;
}