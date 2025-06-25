using Godot;

namespace CS.Nodes.Options;

/// <summary>
/// Responsible for managing user options on game load and afterward
/// </summary>
public partial class OptionsSceneSystem : Control
{
	private const string ConfigFilePath = "user://config_options.cfg";
	private ConfigFile _configFile = new();
	public Control? PreviousScene;
	
	[ExportCategory("Owned")]
	[Export] private AudioStreamPlayer2D? _cancelSound;
	[Export] private BackButtonSystem? _backButton;
	[Export] private ResetButtonSystem? _resetButton;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (_backButton != null)
			_backButton.Pressed += OnBackButtonPressed;
		
		if (_resetButton != null)
			_resetButton.Pressed += OnResetButtonPressed;
		
		LoadConfigs();
	}
	
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel") && IsVisibleInTree())
		{
			SetVisible(false);
			PreviousScene?.SetVisible(true);
			_cancelSound?.Play();
			GetViewport().SetInputAsHandled();
		}
	}

	/// <summary>
	/// Gets the nodes in the config_options group
	/// Loads configs from the <see cref="ConfigFilePath"/>
	/// Saves the values reported by nodes in the config_options group, which ends up saving defaults for missing configs
	/// </summary>
	private void LoadConfigs()
	{
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

	private void OnBackButtonPressed()
	{
		if (PreviousScene == null)
			return;
		
		SetVisible(false);
		PreviousScene.SetVisible(true);
		SaveConfigs();
	}

	/// <summary>
	/// Sets each option in the node group to their default value then saves their values to the config file
	/// </summary>
	private void OnResetButtonPressed()
	{
		var configNodes = GetTree().GetNodesInGroup("config_options");
		foreach (var node in configNodes)
		{
			if (node.HasMethod("LoadConfig"))
				node.Call("ResetToDefault");
			
			if (node.HasMethod("AddToConfig"))
				node.Call("AddToConfig", _configFile);
		}
		
		_configFile.Save(ConfigFilePath);
	}

	/// <summary>
	/// Gets the nodes in the config_options group
	/// Saves the values reported by nodes in the config_options group, which ends up saving defaults for missing configs
	/// </summary>
	private void SaveConfigs()
	{
		var configNodes = GetTree().GetNodesInGroup("config_options");
		foreach (var node in configNodes)
		{
			if (node.HasMethod("AddToConfig"))
				node.Call("AddToConfig", _configFile);
		}
		
		_configFile.Save(ConfigFilePath);
	}
}