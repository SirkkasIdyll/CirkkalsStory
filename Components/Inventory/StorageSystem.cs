using CS.Components.Interaction;
using CS.Nodes.UI.CustomWindow;
using CS.SlimeFactory;
using Godot;

namespace CS.Components.Inventory;

public partial class StorageSystem : NodeSystem
{
    public override void _Ready()
    {
        base._Ready();

        // _nodeManager.SignalBus.InteractWithSignal += OnInteractWith;
    }

    /*private void OnInteractWith(Node<InteractableComponent> node, ref InteractWithSignal args)
    {
        if (!_nodeManager.TryGetComponent<StorageComponent>(node, out var storageComponent))
            return;

        OpenStorageWindow((node, storageComponent));
    }

    private void OpenStorageWindow(Node<StorageComponent> node)
    {
        var customWindow = ResourceLoader.Load<PackedScene>("res://Nodes/UI/CustomWindow/CustomWindow.tscn")
            .Instantiate<CustomWindowSystem>();
        customWindow.Title.Text = "Inventory";
        GetParent().GetNode<CanvasLayer>("CanvasLayer").AddChild(customWindow);
    }*/
}