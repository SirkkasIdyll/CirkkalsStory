using CS.SlimeFactory;

namespace CS.Components.Power;

public partial class PoweredLightSystem : NodeSystem
{
    /// <summary>
    /// TODO: This doesn't need to be in Process, can be achieved with signals
    /// 
    /// </summary>
    /// <param name="delta"></param>
    public override void _Process(double delta)
    {
        base._Process(delta);
        
        _nodeManager.NodeQuery<PoweredLightComponent>(out var dictionary);
        foreach (var (node, comp) in dictionary)
        {
            if (comp.PointLight2D == null)
                return;
            
            if (!_nodeManager.TryGetComponent<PowerCustomerComponent>(node, out var customerComponent))
                continue;

            if (!customerComponent.IsSufficientlyPowered)
            {
                comp.PointLight2D.SetVisible(false);
                continue;
            }
            
            comp.PointLight2D.SetVisible(true);
            comp.PointLight2D.SetTextureScale(comp.LightScale);
            comp.PointLight2D.SetEnergy(comp.LightEnergy);
            comp.PointLight2D.SetColor(comp.LightColor);
        }
    }
}