using Godot;

namespace CS.Nodes.Options;

public partial class VolumeHSliderSystem : HSlider
{
	[Export] private Label _valueLabel; // Label that shows the current volume level
	[Export] private AudioStreamPlayer2D _selectSound; // Sound that plays when volume is changed
	[Export] private string _audioBusName = "Master"; // Which audio bus the slider should control
	[Export] private ResetButtonSystem _resetButton;
	[Export(PropertyHint.Range, "0,100")] private int _defaultVolume = 80;
	
	private int _audioBusIndex; // for manipulating the volume level of a specific bus
	private ConfigData _configData;
	
	public override void _Ready()
	{
		_configData.Section = "Volume";
		_configData.Key = _audioBusName;
		_audioBusIndex = AudioServer.GetBusIndex(_audioBusName);
		
		TooltipText = $"{Value} / {MaxValue}";
		_valueLabel.Text = $"{Value}";
		
		MouseEntered += OnMouseEntered;
		MouseExited += OnMouseExited;
		ValueChanged += OnValueChanged;
		_resetButton.OptionsReset += OnResetButton;
	}
	
	private void OnMouseEntered()
	{
		SetDefaultCursorShape(CursorShape.PointingHand);
	}

	private void OnMouseExited()
	{
		SetDefaultCursorShape(CursorShape.Arrow);
	}
	
	private void OnResetButton()
	{
		Value = _defaultVolume;
	}

	private void OnValueChanged(double value)
	{
		TooltipText = $"{Value} / {MaxValue}";
		_valueLabel.Text = $"{Value}";
		if (!_selectSound.Playing && IsVisibleInTree())
			_selectSound.Playing = true;
		
		_configData.Value = (int) value;
		AudioServer.SetBusVolumeLinear(_audioBusIndex, (float) value / 100); // audioBus value
	}

	private void AddToConfig(ConfigFile config)
	{
		config.SetValue(_configData.Section, _configData.Key, _configData.Value);
	}

	private void LoadConfig(ConfigFile config)
	{
		Value = (int) config.GetValue(_configData.Section, _configData.Key, _defaultVolume);
	}
}