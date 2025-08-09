using CS.SlimeFactory;
using Godot;

namespace CS.Components.Interaction;

public partial class InteractSystem : NodeSystem
{
    public ShaderMaterial OutlineShader = ResourceLoader.Load<ShaderMaterial>("res://Resources/Materials/OutlineCanvasGroup.tres");
    public Vector4 InRangeColor = new Vector4(0.99f, 0.956f, 0.832f, 0.647f);
    public Vector4 OutOfRangeColor = new Vector4(0.811f, 0.327f, 0.289f, 0.779f);
    
    public override void _Ready()
    {
        base._Ready();

        _nodeManager.SignalBus.ShowInteractOutlineSignal += OnShowInteractOutline;
        _nodeManager.SignalBus.HideInteractOutlineSignal += OnHideInteractOutline;
    }

    /*public override void _Input(InputEvent @event)
    {
        if (SpriteGroup == null)
            return;

        if (SpriteGroup.Material != OutlineShader)
            return;

        /*if (!Sprite.IsPixelOpaque(ToLocal(GetGlobalMousePosition())))
            return;#1#

        if ((@event is not InputEventMouseButton inputEventMouse || !inputEventMouse.Pressed ||
             inputEventMouse.ButtonIndex != MouseButton.Left) && !Input.IsActionPressed("interact"))
            return;
                
        if (!_nodeSystemManager.TryGetNodeSystem<PlayerManagerSystem>(out var playerSystem))
            return;
                    
        var signal = new InteractWithSignal(playerSystem.GetPlayer());
        _nodeManager.SignalBus.EmitInteractWithSignal((GetParent(), this), ref signal);
    }*/

    // TODO: This is bad, the single shader is 
    public override void _Process(double delta)
    {
        base._Process(delta);
        
        _nodeManager.NodeQuery<CanInteractComponent>(out var dict);
        foreach (var (owner, component) in dict)
        {
            if (component.InteractTarget == null)
                continue;
            
            if (owner is not Node2D user || component.InteractTarget is not Node2D target)
                continue;

            if (target.GlobalPosition.DistanceTo(user.GlobalPosition) < component.MaxInteractDistance)
                OutlineShader.SetShaderParameter("line_colour", InRangeColor);
            else
                OutlineShader.SetShaderParameter("line_colour", OutOfRangeColor);
            
            var spriteGroup = target.GetNodeOrNull<CanvasGroup>("SpriteGroup");
            
            if (spriteGroup == null)
                continue;

            spriteGroup.Material = OutlineShader;
        }
    }

    /// Something something process CanInteractComponent, get range, set color
    private void OnShowInteractOutline(Node<CanInteractComponent> node, ref ShowInteractOutlineSignal args)
    {
        if (!_nodeManager.HasComponent<InteractableComponent>(args.InteractTarget))
            return;
        
        node.Comp.InteractTarget = args.InteractTarget;
    }
    
    private void OnHideInteractOutline(Node<CanInteractComponent> node, ref HideInteractOutlineSignal args)
    {
        if (node.Comp.InteractTarget == args.InteractTarget)
            node.Comp.InteractTarget = null;
        
        var spriteGroup = args.InteractTarget.GetNodeOrNull<CanvasGroup>("SpriteGroup");

        if (spriteGroup == null)
            return;
        
        spriteGroup.Material = null;
    }
}