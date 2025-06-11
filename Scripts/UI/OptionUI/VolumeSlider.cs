using Godot;

namespace CS.Scripts.UI.OptionUI;

public partial class VolumeSlider : HSlider
{
	[Export] private Label _valueLabel; // Label that shows the current volume level
	[Export] private AudioStreamPlayer2D _selectSound; // Sound that plays when volume is changed
	[Export] private string _audioBusName = "Master"; // Which audio bus the slider should control
	[Export] private ResetButton _resetButton;
	[Export(PropertyHint.Range, "0,100")] private int _defaultVolume = 100;
	
	private int _audioBusIndex;
	private int _initialVolume;
	
	public override void _Ready()
	{
		SetToDefaultVolume();
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
		SetToDefaultVolume();
	}

	private void OnValueChanged(double value)
	{
		TooltipText = $"{Value} / {MaxValue}";
		_valueLabel.Text = $"{Value}";
		if (!_selectSound.Playing)
			_selectSound.Playing = true;
		
		AudioServer.SetBusVolumeLinear(_audioBusIndex, (float) value / 100);
	}

	private void SetToDefaultVolume()
	{
		Value = _defaultVolume;
		AudioServer.SetBusVolumeLinear(_audioBusIndex, (float) _defaultVolume / 100);
	}
}