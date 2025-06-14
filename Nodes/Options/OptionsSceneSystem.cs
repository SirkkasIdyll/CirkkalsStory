using CS.Nodes.StartMenu;
using Godot;

namespace CS.Nodes.Options;

public partial class OptionsSceneSystem : AspectRatioContainer
{
	public const string ConfigFilePath = "user://config_options.cfg";

	[Export] private StartMenuSceneSystem _startMenu;
	[Export] private AudioStreamPlayer2D _cancelSound;
	private ConfigFile _configFile;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_configFile = new ConfigFile();
		var error = _configFile.Load(ConfigFilePath);
		var configNodes = GetTree().GetNodesInGroup("config_options");
		foreach (var node in configNodes)
		{
			if (node.HasMethod("LoadConfig"))
				node.Call("LoadConfig", _configFile);
			
			if (node.HasMethod("AddToConfig"))
				node.Call("AddToConfig", _configFile);
		}
		
		_configFile.Save(ConfigFilePath);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel") && Visible)
		{
			_startMenu.Visible = true;
			Visible = false;
			_cancelSound.Playing = true;
			GetViewport().SetInputAsHandled();
			
			var configNodes = GetTree().GetNodesInGroup("config_options");
			foreach (var node in configNodes)
			{
				if (node.HasMethod("AddToConfig"))
					node.Call("AddToConfig", _configFile);
			}
		
			_configFile.Save(ConfigFilePath);
		}
	}
}

/// <summary>
/// </summary>
/// <param name="section">General category to store under</param>
/// <param name="key">Specific key to reference</param>
/// <param name="value">What information is relevant to store</param>
struct ConfigData(string section, string key, Variant value)
{
	/// <summary>
	/// What section to save the settings in
	/// </summary>
	public string Section = section;

	/// <summary>
	/// What name to save the setting under
	/// </summary>
	public string Key = key;

	/// <summary>
	/// What information to save
	/// Because this is a variant type, the value will need to be type-hinted when loaded
	/// </summary>
	public Variant Value = value;
}