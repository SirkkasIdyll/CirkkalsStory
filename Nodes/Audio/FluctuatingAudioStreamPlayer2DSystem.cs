using System;
using Godot;

namespace CS.Nodes.Audio;

/// <summary>
/// Causes the pitch to randomly fluctuate every time the audio stream is played
/// </summary>
public partial class FluctuatingAudioStreamPlayer2DSystem : AudioStreamPlayer2D
{
    /// <summary>
    /// How much lower than 1.0 the pitch can fluctuate
    /// </summary>
    [Export(PropertyHint.Range, "-99, 0")]
    private int _pitchFluctuationMin = -5;
    
    /// <summary>
    /// How much higher than 1.0 the pitch can fluctuate
    /// </summary>
    [Export(PropertyHint.Range, "0,300")]
    private int _pitchFluctuationMax = 5;
    
    public override void _Ready()
    {
        Finished += OnFinished;
    }

    /// <summary>
    /// In case someone forgets to use Play(), we randomize the pitch again when the audio finishes playing
    /// </summary>
    private void OnFinished()
    {
        RandomizePitch(_pitchFluctuationMin, _pitchFluctuationMax);
    }
    
    /// <summary>
    /// Overloaded Play() that randomizes the pitch before audio is played
    /// </summary>
    private void Play()
    {
        RandomizePitch(_pitchFluctuationMin, _pitchFluctuationMax);
        Play(0F);
    }
    
    /// <summary>
    /// Fluctuate pitch from default 1.0 based off of min and max divided by 100 (1 = 0.01)
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    private void RandomizePitch(int min, int max)
    {
        var randomizedPitchScale = 1 + (float) Random.Shared.Next(min, max) / 100;
        SetPitchScale(randomizedPitchScale);
    }
}