using CS.Components.Grid;
using CS.Components.Interaction;
using CS.SlimeFactory;
using Godot;

namespace CS.Components.UI;

public partial class UserInterfaceSystem : NodeSystem
{
    [InjectDependency] private readonly GridSystem _gridSystem = null!;
    
    public override void _Ready()
    {
        base._Ready();

        _nodeManager.SignalBus.InteractWithSignal += OnInteractWith;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        
        // Check how far user is from the object that created the UI
        // and close the UI if they get too far
        _nodeManager.NodeQuery<AttachedUserInterfaceComponent>(out var dict);
        foreach (var (owner, component) in dict)
        {
            if (component.User == null)
                continue;

            if (owner is not Node2D objectNode || component.User is not Node2D userNode)
                continue;
            
            if (!_gridSystem.TryGetDistance(userNode, objectNode, out var distance))
                continue;
            
            if (distance < component.MaxUseDistance)
                continue;
            
            CloseAttachedUserInterface((owner, component));
        }
    }

    private void OnInteractWith(Node<InteractableComponent> node, ref InteractWithSignal args)
    {
        if (!_nodeManager.TryGetComponent<AttachedUserInterfaceComponent>(node, out var attachedUserInterfaceComponent))
            return;
        
        OpenAttachedUserInterface((node, attachedUserInterfaceComponent), args.Interactee);
    }
    
    /// <summary>
    /// Open the attached user interface and prevent opening duplicates
    /// </summary>
    private void OpenAttachedUserInterface(Node<AttachedUserInterfaceComponent> node, Node user)
    {
        // Already a UI open, don't open another one
        if (node.Comp.UserInterface != null)
            return;
        
        node.Comp.UserInterface = node.Comp.UserInterfaceScene.Instantiate<Control>();
        node.Comp.User = user;
        node.Comp.UserInterface.TreeExited += () => CloseAttachedUserInterface(node);
        CanvasLayer.AddChild(node.Comp.UserInterface);
    }

    /// <summary>
    /// 
    /// </summary>
    private void CloseAttachedUserInterface(Node<AttachedUserInterfaceComponent> node)
    {
        if (node.Comp.UserInterface == null)
            return;
        
        if (!node.Comp.UserInterface.IsQueuedForDeletion())
            node.Comp.UserInterface.QueueFree();
        
        node.Comp.UserInterface = null;
        node.Comp.User = null;
    }
}