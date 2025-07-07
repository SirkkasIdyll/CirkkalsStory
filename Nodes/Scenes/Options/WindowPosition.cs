using Godot;

namespace CS.Nodes.Scenes.Options;

public partial class WindowPosition : Control, IConfigSystem
{
	private ConfigData _configData;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_configData.Section = "Window";
		_configData.Key = "Position";
	}
	
	public void AddToConfig(ConfigFile configFile)
	{
		_configData.Value = GetWindow().GetPosition();
		configFile.SetValue(_configData.Section, _configData.Key, _configData.Value);
	}

	public void LoadConfig(ConfigFile configFile)
	{
		var value = configFile.GetValue(_configData.Section, _configData.Key, GetWindow().GetPosition());
		GetWindow().SetPosition(value.AsVector2I());
	}

	public void ResetToDefault()
	{
	}
}