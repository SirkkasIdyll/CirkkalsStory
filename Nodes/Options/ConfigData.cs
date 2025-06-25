using Godot;

namespace CS.Nodes.Options;

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