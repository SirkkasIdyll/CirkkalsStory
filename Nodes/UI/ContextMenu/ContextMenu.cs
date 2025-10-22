using Godot;

namespace CS.Nodes.UI.ContextMenu;

public partial class ContextMenu : PopupMenu
{
    /// <summary>
    /// Close the window on any click outside of it
    /// </summary>
    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
		
        if (@event is not InputEventMouseButton mouseEvent)
            return;

        if (!mouseEvent.Pressed)
            return;
        
        if (!new Rect2(Position, Size).HasPoint(GetViewport().GetMousePosition()))
            QueueFree();
    }

    public override void _Ready()
    {
        base._Ready();
        
        SetPosition((Vector2I)GetViewport().GetMousePosition());
        SetVisible(true);
        SetSize(new Vector2I(0, 0)); // Set size to the smallest possible so it automatically resizes as items are added
    }

    /// <summary>
    /// Checks if the id of the index pressed is equal to the action desired
    /// </summary>
    public bool IndexIdMatchesAction(int index, ContextMenuAction action)
    {
        return GetItemId(index) == (int)action;
    }
}

/// <summary>
/// To easily differentiate between actions in context menus,
/// it can be assigned a specific ID through this enum,
/// which can then be checked for using get_item_id(index) after subscribing to index_pressed
/// </summary>
public enum ContextMenuAction
{
    Drop,
    Equip,
    Interact,
    Unequip,
    Pull,
    StopPull
}