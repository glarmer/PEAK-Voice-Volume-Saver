using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using VoiceVolumeSaver.Patches;

namespace VoiceVolumeSaver;

[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    private readonly Harmony _harmony = new Harmony(Id);
    public static ConfigurationHandler ConfigurationHandler;

    private void Awake()
    {
        Logger = base.Logger;
        ConfigurationHandler = new ConfigurationHandler(Config);
        
        Logger.LogInfo($"Plugin {Id} is patching Init!");
        Logger.LogInfo($"Plugin {Id} is patching OnSliderChanged!");
        _harmony.PatchAll(typeof(AudioLevelSliderPatches));
        _harmony.PatchAll(typeof(OnPlayerEnteredRoomPatch));
        Logger.LogInfo($"Plugin {Id} has finished loading!");
    }
    
}