using CS.Components.CameraAim;
using Godot;

namespace CS.SlimeFactory.Signals;

public partial class SignalBus
{
    public delegate void CurrentlyAimingSignalHandler(CharacterBody2D characterBody2D, ref CurrentlyAimingSignal args);
    public event CurrentlyAimingSignalHandler? CurrentlyAimingSignal;
    public void EmitCurrentlyAimingSignal(CharacterBody2D characterBody2D, ref CurrentlyAimingSignal args)
    {
        CurrentlyAimingSignal?.Invoke(characterBody2D, ref args);
    }
}