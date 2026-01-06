using Godot;

namespace PC.Nodes.Scenes.Options;

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

public interface IConfigSystem
{
    /// <summary>
    /// Call <see cref="ConfigFile"/>'s SetValue() with the corresponding section, key, and value
    /// </summary>
    public void AddToConfig(ConfigFile configFile);
    
    /// <summary>
    /// Call <see cref="ConfigFile"/>'s GetValue() with the corresponding section, key, and default value to fallback to
    /// </summary>
    /// <param name="configFile"></param>
    public void LoadConfig(ConfigFile configFile);
    
    /// <summary>
    /// Default value to fallback to when the ResetButton is pressed
    /// </summary>
    public void ResetToDefault();
}