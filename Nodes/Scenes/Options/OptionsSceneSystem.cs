using CS.Nodes.UI.ButtonTypes;
using Godot;

namespace CS.Nodes.Scenes.Options;

/// <summary>
/// Responsible for managing user options on game load and afterward
/// </summary>
public partial class OptionsSceneSystem : Control
{
	private const string ConfigFilePath = "user://config_options.cfg";
	private ConfigFile _configFile = new();
	public Control? PreviousScene;
	
	[ExportCategory("Owned")]
	[Export] private AudioStreamPlayer2D _cancelSound = null!;
	[Export] private StandardButton _backButton = null!;
	[Export] private StandardButton _resetButton = null!;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_backButton.Pressed += OnBackButtonPressed;
		_resetButton.Pressed += OnResetButtonPressed;
		
		LoadConfigs();
	}
	
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel") && IsVisibleInTree())
		{
			OnBackButtonPressed();
			GetViewport().SetInputAsHandled();
		}
	}

	/// <summary>
	/// Hide the options scene, make the previous scene visible again, and save new configs to file
	/// </summary>
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
		// To add a node to the config_options group, use the Godot Editor
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