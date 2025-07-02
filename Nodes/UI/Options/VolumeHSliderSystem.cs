using CS.Nodes.UI.Audio;
using Godot;

namespace CS.Nodes.UI.Options;

public partial class VolumeHSliderSystem : HSlider
{
	private int _audioBusIndex; // for manipulating the volume level of a specific bus
	private ConfigData _configData;
	
	[ExportCategory("Instantiated")]
	[Export(PropertyHint.Enum, "Master,Music,SFX")]  private string _audioBusName = "Master";
	[Export(PropertyHint.Range, "0,100")]  private int _defaultVolume = 60;
	
	[ExportCategory("Owned")]
	[Export] private Label? _valueLabel; // Label that shows the current volume level
	[Export] private FluctuatingAudioStreamPlayer2DSystem? _selectSound; // Sound that plays when volume is changed
	
	public override void _Ready()
	{
		_configData.Section = "Volume";
		_configData.Key = _audioBusName;
		_audioBusIndex = AudioServer.GetBusIndex(_audioBusName);
		
		TooltipText = $"{Value} / {MaxValue}";
		if (_valueLabel != null)
			_valueLabel.Text = $"{Value}";

		MouseEntered += OnMouseEntered;
		MouseExited += OnMouseExited;
		ValueChanged += OnValueChanged;
	}
	
	/// <summary>
	/// Adds the audio slider's setting into the appropriate section, key, and value in the ConfigFile
	/// </summary>
	/// <param name="config">The helper class that saves the config values</param>
	private void AddToConfig(ConfigFile config)
	{
		config.SetValue(_configData.Section, _configData.Key, _configData.Value);
	}
	
	/// <summary>
	/// Grabs the volume from the appropriate section and key of the ConfigFile,
	/// or resorts to the <see cref="_defaultVolume"/> if the config is missing                                                                                                                        
	///
	/// Loading this config will update the Value of the slider,
	/// which will in turn update the corresponding AudioBus in <see cref="OnValueChanged"/>
	/// </summary>
	/// <param name="config">The helper class that saves the config values</param>
	private void LoadConfig(ConfigFile config)
	{
		Value = (int) config.GetValue(_configData.Section, _configData.Key, _defaultVolume);
	}
	
	private void OnMouseEntered()
	{
		SetDefaultCursorShape(CursorShape.PointingHand);
	}

	private void OnMouseExited()
	{
		SetDefaultCursorShape(CursorShape.Arrow);
	}

	/// <summary>
	/// Updates the AudioBus volume level,
	/// sets the tooltip and label text,
	/// plays a feedback sound,
	/// and updates the value to be saved into config
	/// </summary>
	/// <param name="value">The percentage to set the volume to</param>
	private void OnValueChanged(double value)
	{
		// Set the actual bus to the volume of the slider
		AudioServer.SetBusVolumeLinear(_audioBusIndex, (float) value / 100);
        
		// Update the tooltip and label indicating current volume level
		TooltipText = $"{Value} / {MaxValue}";
		if (_valueLabel != null)
			_valueLabel.Text = $"{Value}";
		
		// Play the feedback sound when the user changes the volume level
		if (_selectSound != null)
			if (!_selectSound.Playing && IsVisibleInTree())
				_selectSound.Play();
		
		// Update the save data config value, but doesn't save it
		_configData.Value = (int) value;
	}
	
	/// <summary>
	/// Sets the volume back to default
	/// Called when reset button is triggered
	/// </summary>
	private void ResetToDefault()
	{
		Value = _defaultVolume;
	}
}