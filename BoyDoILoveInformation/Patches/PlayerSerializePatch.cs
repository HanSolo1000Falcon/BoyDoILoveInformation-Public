using System;
using HarmonyLib;

namespace BoyDoILoveInformation.Patches;

[HarmonyPatch(typeof(VRRig), nameof(VRRig.SerializeReadShared))]
public static class PlayerSerializePatch
{
    public static bool Prefix() => true;

    public static Action<VRRig> OnPlayerSerialize;

    public static void Postfix(VRRig __instance, InputStruct data) =>
            OnPlayerSerialize?.Invoke(__instance);
}