using Godot;
using PC.Nodes.UI.Audio;
using PC.SlimeFactory;

namespace PC.Components.Movement;

public partial class MovementComponent : Component
{
    [Export] 
    public int WalkingSpeed = 80;
    
    [Export]
    public int RunningSpeed = 120;
    
    [Export]
    public LoopingAudioStreamPlayer2DSystem? SoundEffect;
}