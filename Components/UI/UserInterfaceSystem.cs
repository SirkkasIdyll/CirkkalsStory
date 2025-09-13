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
            foreach (var (action, user) in component.UserUsingInterface)
            {
                if (owner is not Node2D objectNode || user is not Node2D userNode)
                    continue;
                
                if (!_gridSystem.TryGetDistance(userNode, objectNode, out var distance))
                    continue;
            
                if (distance < component.MaxUseDistance)
                    continue;
            
                CloseAttachedUserInterface((owner, component), action);
            }
        }
    }

    private void OnInteractWith(Node<InteractableComponent> node, ref InteractWithSignal args)
    {
        if (!_nodeManager.HasComponent<OpenAttachedUserInterfaceOnInteractComponent>(node))
            return;
        
        if (!_nodeManager.TryGetComponent<AttachedUserInterfaceComponent>(node, out var attachedUserInterfaceComponent))
            return;
        
        OpenAttachedUserInterface((node, attachedUserInterfaceComponent), args.Interactee, "interact");
    }
    
    /// <summary>
    /// Open the attached user interface and prevent opening duplicates
    /// </summary>
    private void OpenAttachedUserInterface(Node<AttachedUserInterfaceComponent> node, Node user, string action)
    {
        // Already a UI open, don't open another one
        if (node.Comp.UserInterface.ContainsKey(action))
            return;

        node.Comp.UserInterface[action] = node.Comp.UserInterfaceScenes[action].Instantiate<Control>();
        node.Comp.UserUsingInterface[action] = user;
        node.Comp.UserInterface[action].TreeExited += () => CloseAttachedUserInterface(node, action);
        CanvasLayer.AddChild(node.Comp.UserInterface[action]);
    }

    /// <summary>
    /// Closes the attached user interface
    /// </summary>
    private void CloseAttachedUserInterface(Node<AttachedUserInterfaceComponent> node, string action)
    {
        if (!node.Comp.UserInterface.ContainsKey(action))
            return;
        
        if (!node.Comp.UserInterface[action].IsQueuedForDeletion())
            node.Comp.UserInterface[action].QueueFree();
        
        node.Comp.UserInterface.Remove(action);
        node.Comp.UserUsingInterface.Remove(action);
    }
}