using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;


namespace VoiceVolumeSaver;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    private readonly Harmony _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
    private static Dictionary<string, float> _voiceVolumes = new Dictionary<string, float>();
    private static ConfigEntry<string> configDictionary;
    private static Plugin plugin;

    private void Awake()
    {
        plugin = this;
        configDictionary = Config.Bind(
            "General",
            "SavedData",
            "",
            "Serialized key=value pairs"
        );
        Logger = base.Logger;
        if (configDictionary.Value != "") {
            LoadDictionaryFromConfig();
        }
        
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is patching Init!");
        _harmony.PatchAll(typeof(InitPatch));
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is patching OnSliderChanged!");
        _harmony.PatchAll(typeof(OnSliderChangedPatch));
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} has finished loading!");
    }
    
    private void LoadDictionaryFromConfig()
    {
        _voiceVolumes = DeserializeDictionary(configDictionary.Value);
    }
    
    public static Dictionary<string, float> DeserializeDictionary(string data)
    {
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is deserializing data!");
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
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is serializing data!");
        return string.Join(";", _voiceVolumes.Select(kvp => $"{(kvp.Key)}={kvp.Value}"));
    }

    private void SaveDictionaryToConfig()
    {
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is pre serializing data!");
        configDictionary.Value = SerializeDictionary();
        Config.Save(); // Make sure to write to disk
    }
    
    public class InitPatch
    {
        [HarmonyPatch(typeof(AudioLevelSlider), "Init")]
        [HarmonyPostfix]
        static void Postfix(AudioLevelSlider __instance)
        {
            Logger.LogInfo($"Player Joined: {__instance.player.NickName}");
            if (_voiceVolumes.TryGetValue(__instance.player.NickName, out var savedVolume))
            {
                Logger.LogInfo($"Player Volume: {savedVolume}");
                AudioLevels.SetPlayerLevel(__instance.player.ActorNumber, savedVolume); 
                __instance.slider.SetValueWithoutNotify(AudioLevels.GetPlayerLevel(__instance.player.ActorNumber));
                __instance.percent.text = Mathf.RoundToInt(__instance.slider.value * 200f).ToString() + "%";
            }
        }
    }
    
    public class OnSliderChangedPatch
    {
        [HarmonyPatch(typeof(AudioLevelSlider), "OnSliderChanged")]
        [HarmonyPostfix]
        static void Postfix(float newValue, AudioLevelSlider __instance)
        {
            //save newvalue to text file
            Logger.LogInfo($"Player saving: {__instance.player.NickName} {newValue}");
            _voiceVolumes.Add(__instance.player.NickName, newValue);
            plugin.SaveDictionaryToConfig();
        }
    }
    
}
