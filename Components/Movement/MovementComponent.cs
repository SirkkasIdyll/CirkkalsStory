using CS.Nodes.UI.Audio;
using CS.SlimeFactory;
using Godot;

namespace CS.Components.Movement;

public partial class MovementComponent : Component
{
    [Export] 
    public int WalkingSpeed = 80;
    
    [Export]
    public int RunningSpeed = 120;
    
    [Export]
    public LoopingAudioStreamPlayer2DSystem? SoundEffect;
}