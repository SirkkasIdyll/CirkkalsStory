using Godot;

namespace CS.Nodes.Scenes.Options;

public partial class ResolutionOptionButtonSystem : OptionButton, IConfigSystem
{
    private ConfigData _configData;
    private string _defaultValue = "1920x1080";

    public override void _Ready()
    {
        base._Ready();

        _configData.Section = "Resolution";
        _configData.Key = "Size";

        ItemSelected += OnItemSelected;
    }

    private void OnItemSelected(long index)
    {
        _configData.Value = GetItemText((int) index);
        var value = GetItemText((int) index).Split("x");
        var height = int.Parse(value[0]);
        var width = int.Parse(value[1]);
        GetWindow().SetSize(new Vector2I(height, width));
    }

    public void AddToConfig(ConfigFile configFile)
    {
        configFile.SetValue(_configData.Section, _configData.Key, _configData.Value);
    }

    public void LoadConfig(ConfigFile configFile)
    {
        var value = configFile.GetValue(_configData.Section, _configData.Key, _defaultValue);
        for (int i = 0; i < GetItemCount(); i++)
        {
            if ((string)value != GetItemText(i))
                continue;
            
            Select(i);
            OnItemSelected(i);
        }
    }

    public void ResetToDefault()
    {
        for (int i = 0; i < GetItemCount(); i++)
        {
            if (_defaultValue != GetItemText(i))
                continue;
            
            Select(i);
            OnItemSelected(i);
        }
    }
}