using CS.SlimeFactory;
using Godot;

namespace CS.Components.Power;

public partial class PoweredLightComponent : Component
{
    [Export] 
    public PointLight2D? PointLight2D = null;
    
    /// <summary>
    /// The scale of the light's texture, which increases the range of the light
    /// </summary>
    [Export]
    public float LightScale = 1.0f;
    
    /// <summary>
    /// How powerful the light effect is in the area it affects
    /// </summary>
    [Export]
    public float LightEnergy = 1.0f;
    
    /// <summary>
    /// What color the light should be
    /// </summary>
    [Export]
    public Color LightColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
}