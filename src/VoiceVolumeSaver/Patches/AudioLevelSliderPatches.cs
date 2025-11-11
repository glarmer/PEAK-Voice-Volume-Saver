using HarmonyLib;
using UnityEngine;

namespace VoiceVolumeSaver.Patches;

[HarmonyPatch]
public class AudioLevelSliderPatches
{
    
    
    [HarmonyPatch(typeof(AudioLevelSlider), "Init")]
    [HarmonyPostfix]
    static void InitPostfix(AudioLevelSlider __instance)
    {
        if (__instance.player == null)
            return;

        Plugin.Logger.LogInfo($"Player Joined: {__instance.player.NickName}");

        if (__instance.player != null && !__instance.player.IsLocal)
        {
            if (ConfigurationHandler.VoiceVolumes.TryGetValue(__instance.player.NickName, out var savedVolume))
            {
                Plugin.Logger.LogInfo($"Player Volume: {savedVolume}");
                AudioLevels.SetPlayerLevel(__instance.player.UserId, savedVolume);
                __instance.slider.SetValueWithoutNotify(AudioLevels.GetPlayerLevel(__instance.player.UserId));
                __instance.percent.text = Mathf.RoundToInt(__instance.slider.value * 200f) + "%";
            }
        }
    }

    [HarmonyPatch(typeof(AudioLevelSlider), "OnSliderChanged")]
    [HarmonyPostfix]
    static void OnSliderChangedPostfix(float newValue, AudioLevelSlider __instance)
    {
        if (__instance.player == null)
            return;

        Plugin.Logger.LogInfo($"Player saving: {__instance.player.NickName} {newValue}");

        ConfigurationHandler.VoiceVolumes[__instance.player.NickName] = newValue;

        Plugin.ConfigurationHandler.SaveDictionaryToConfig();
    }
}
