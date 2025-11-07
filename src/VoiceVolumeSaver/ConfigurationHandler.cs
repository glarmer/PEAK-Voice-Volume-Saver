using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;

namespace VoiceVolumeSaver;

public class ConfigurationHandler
{
    private ConfigFile _config;
    private static ConfigEntry<string> _configDictionary;
    public static Dictionary<string, float> VoiceVolumes { get; private set; } = new Dictionary<string, float>();
    
    
    public ConfigurationHandler(ConfigFile configFile)
    {
        _config = configFile;
        
        Plugin.Logger.LogInfo("ConfigurationHandler initialising");
        _configDictionary = _config.Bind(
            "General",
            "SavedData",
            "",
            "Serialized key=value pairs"
        );
        
        if (_configDictionary.Value != "") {
            LoadDictionaryFromConfig();
        }
        
        Plugin.Logger.LogInfo("ConfigurationHandler initialised");
    }
     
    private void LoadDictionaryFromConfig()
    {
        VoiceVolumes = DeserializeDictionary(_configDictionary.Value);
    }
     
    public static Dictionary<string, float> DeserializeDictionary(string data)
    {
        Plugin.Logger.LogInfo($"Plugin {Plugin.Id} is deserializing data!");
        var dict = new Dictionary<string, float>();

        if (string.IsNullOrWhiteSpace(data))
            return dict;

        var pairs = data.Split(';');
        foreach (var pair in pairs)
        {
            var parts = pair.Split('=');
            if (parts.Length != 2)
                continue;

            string key = (parts[0]);
            if (float.TryParse(parts[1], out float value))
            {
                dict[key] = value;
            }
        }

        return dict;
    }
    
    public string SerializeDictionary()
    {
        // Format: key=1.0;key2=2.5;key3=0.75
        Plugin.Logger.LogInfo($"Plugin {Plugin.Id} is serializing data!");
        return string.Join(";", VoiceVolumes.Select(kvp => $"{(kvp.Key)}={kvp.Value}"));
    }

    public void SaveDictionaryToConfig()
    {
        Plugin.Logger.LogInfo($"Plugin {Plugin.Id} is pre serializing data!");
        _configDictionary.Value = SerializeDictionary();
        _config.Save(); // Make sure to write to disk
    }
}