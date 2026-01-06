using System.Collections.Generic;
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
            foreach (var (action, uiDict) in component.ActiveUserInterfaces)
            {
                foreach (var (user, control) in uiDict)
                {
                    if (owner is not Node2D objectNode || user is not Node2D userNode)
                        continue;
                
                    if (!_gridSystem.TryGetDistance(userNode, objectNode, out var distance))
                        continue;
            
                    if (distance < component.MaxUseDistance)
                        continue;
            
                    CloseAttachedUserInterface((owner, component), user, action);
                }
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
    /// Open the attached user interface for the given action
    /// Or closes it if it's already opened by a user
    /// </summary>
    public Control? OpenAttachedUserInterface(Node<AttachedUserInterfaceComponent> node, Node user, string action)
    {
        // If we aren't allowing simultaneous use, check if open attempt was done by the current active user
        if (!node.Comp.AllowSimultaneousUse && node.Comp.ActiveUserInterfaces.Count != 0)
        {
            var userMatchesActiveUser = false;
            foreach (var dict in node.Comp.ActiveUserInterfaces.Values)
            {
                if (!dict.ContainsKey(user))
                    continue;
                
                userMatchesActiveUser = true;
                break;
            }
            
            // Checked every open UI and user who is attempting to open this UI is not the currently active user
            if (!userMatchesActiveUser)
                return null;
        }

        
        // If the request is from an active user with the UI open, toggle the UI closed to feel responsive
        if (node.Comp.ActiveUserInterfaces[action].ContainsKey(user))
        {
            CloseAttachedUserInterface(node, user, action);
            return null;
        }

        // Open the UI
        var control = node.Comp.UserInterfaceScenes[action].Instantiate<Control>();
        control.TreeExited += () => CloseAttachedUserInterface(node, user, action);
        CanvasLayer.AddChild(control);
        
        if (node.Comp.ActiveUserInterfaces.TryGetValue(action, out var uiDict))
            uiDict.Add(user, control);
        else
            node.Comp.ActiveUserInterfaces.Add(action, new Dictionary<Node, Control>() {{user, control}});
        
        return control;
    }

    /// <summary>
    /// Closes the attached user interface for the given action
    /// </summary>
    public void CloseAttachedUserInterface(Node<AttachedUserInterfaceComponent> node, Node user, string action)
    {
        if (!node.Comp.ActiveUserInterfaces.TryGetValue(action, out var uiDict))
            return;
        
        // Close UI
        if (!uiDict[user].IsQueuedForDeletion())
            uiDict[user].QueueFree();

        // Remove user from active user interfaces of this kind
        uiDict.Remove(user);
        
        // If no users have this kind of interface open, remove the key entirely
        if (uiDict.Count == 0)
            node.Comp.ActiveUserInterfaces.Remove(action);
    }
}