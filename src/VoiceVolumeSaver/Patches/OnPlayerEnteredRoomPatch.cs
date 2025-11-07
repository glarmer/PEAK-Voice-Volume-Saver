using System;
using BepInEx;
using HarmonyLib;
using Photon.Pun;

namespace VoiceVolumeSaver.Patches;

public class OnPlayerEnteredRoomPatch
{
    [HarmonyPatch(typeof(PlayerConnectionLog), "OnPlayerEnteredRoom")]
    [HarmonyPostfix]
    static void Postfix()
    {
        bool isActive = GUIManager.instance.pauseMenu.activeSelf;
        GUIManager.instance.pauseMenu.SetActive(true);
        GUIManager.instance.pauseMenu.SetActive(isActive);
    }
}