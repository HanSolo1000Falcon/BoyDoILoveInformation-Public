using BoyDoILoveInformation.Tools;
using HarmonyLib;

namespace BoyDoILoveInformation.Patches;

[HarmonyPatch(typeof(VRRigCache), nameof(VRRigCache.RemoveRigFromGorillaParent))]
public class PlayerRigCachedPatch
{
    private static void Postfix(NetPlayer player, VRRig vrrig)
    {
        BDILIUtils.OnPlayerRigCached?.Invoke(vrrig);
        
        Extensions.PlayersWithCosmetics.Remove(vrrig);
        Extensions.PlayerPlatforms.Remove(vrrig);
        Extensions.PlayerMods.Remove(vrrig);
        PunCallbacks.CheatsNotifiedAbout.Remove(vrrig);
        BDILIUtils.HasNotifiedAboutCosmetX.Remove(vrrig);
        if (VoicePrioritizationPatch.PrioritizedPeople.Contains(vrrig)) VoicePrioritizationPatch.PrioritizedPeople.Remove(vrrig);
    }
}