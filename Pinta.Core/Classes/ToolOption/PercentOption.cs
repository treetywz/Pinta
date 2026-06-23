namespace Pinta.Core;

/// <summary>
/// A 0-100 percent-valued option that creates a slider.
/// </summary>
public class PercentOption : ToolOption
{
	private string name;
	private int value;
	public string LabelText { get; private set; }

	public int Value {
		get => value;
		set {
			this.value = value;
			OnValueChanged?.Invoke (Value);
		}
	}

	public delegate void ValueChange (int newValue);
	public event ValueChange? OnValueChanged;

	public PercentOption (string name, int initialValue, string labelText)
	{
		this.name = name;
		LabelText = labelText;
		Value = initialValue;
	}

	public string GetUniqueName () => name;

	public void LoadValueFromSettings (ISettingsService settingsService)
	{
		int invalidValue = -1;
		int savedValue = settingsService.GetSetting<int> (name, invalidValue);
		if (savedValue != invalidValue)
			Value = savedValue;
	}

	public void SaveValueToSettings (ISettingsService settingsService)
	{
		settingsService.PutSetting (name, Value);
	}
}