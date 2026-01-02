using System;
using BoyDoILoveInformation.Tools;
using HarmonyLib;
using Photon.Pun;

namespace BoyDoILoveInformation.Patches;

[HarmonyPatch(typeof(VRRig), nameof(VRRig.SerializeReadShared))]
public static class PlayerSerializePatch
{
    private static void Postfix(VRRig __instance, InputStruct data)
    {
        double ping     = Math.Abs((__instance.velocityHistoryList[0].time - PhotonNetwork.Time) * 1000);
        int    safePing = (int)Math.Clamp(Math.Round(ping), 0, int.MaxValue);

        Extensions.PlayerPing[__instance] = safePing;
    }
}