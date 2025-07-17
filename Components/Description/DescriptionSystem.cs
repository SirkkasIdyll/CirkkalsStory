using CS.SlimeFactory;
using Godot;

namespace CS.Components.Description;

public partial class DescriptionSystem : NodeSystem
{
    public override void _Ready()
    {
        base._Ready();

    }

    public string GetDisplayName(Node node)
    {
        if (!_nodeManager.TryGetComponent<DescriptionComponent>(node, out var descriptionComponent))
            return "DEFAULT_NAME";

        if (descriptionComponent.DisplayName == "")
            GD.Print(node.Name + " has no display name set");
        
        return descriptionComponent.DisplayName;
    }
}