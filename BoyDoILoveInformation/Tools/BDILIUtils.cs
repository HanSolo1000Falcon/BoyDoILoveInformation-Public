using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BoyDoILoveInformation.Core;
using GorillaLocomotion;
using GorillaNetworking;
using Newtonsoft.Json;
using UnityEngine;

namespace BoyDoILoveInformation.Tools;

public class BDILIUtils : MonoBehaviour
{
    public static Action<VRRig> OnPlayerCosmeticsLoaded;
    public static Action<VRRig> OnPlayerRigCached;

    public static Transform RealRightController;
    public static Transform RealLeftController;

    public static Dictionary<VRRig, bool> HasNotifiedAboutCosmetX = new();

    public static readonly ConcurrentDictionary<string, bool> OptOutCache = new();

    private void Start()
    {
        RealRightController = new GameObject("RealRightController").transform;
        RealLeftController  = new GameObject("RealLeftController").transform;
    }

    private void LateUpdate()
    {
        RealRightController.position =
                GTPlayer.Instance.RightHand.controllerTransform.TransformPoint(GTPlayer.Instance.RightHand.handOffset);

        RealLeftController.position =
                GTPlayer.Instance.LeftHand.controllerTransform.TransformPoint(GTPlayer.Instance.LeftHand.handOffset);

        RealRightController.rotation =
                GTPlayer.Instance.RightHand.controllerTransform.rotation * GTPlayer.Instance.RightHand.handRotOffset;

        RealLeftController.rotation =
                GTPlayer.Instance.LeftHand.controllerTransform.rotation * GTPlayer.Instance.LeftHand.handRotOffset;

        if (!GorillaParent.hasInstance || GorillaParent.instance.vrrigs == null)
            return;

        _ = DoCosmetXChecks();
    }

    private static async Task DoCosmetXChecks()
    {
        foreach (VRRig rig in GorillaParent.instance.vrrigs)
        {
            bool isPlayerOptedOut = await IsPlayerOptedOut(rig.OwningNetPlayer.UserId);

            if (isPlayerOptedOut)
                continue;
            
            if (!Extensions.PlayerMods.ContainsKey(rig))
                Extensions.PlayerMods[rig] = [];

            if (!rig.HasCosmetics())
                continue;

            HasNotifiedAboutCosmetX.TryAdd(rig, false);

            CosmeticsController.CosmeticSet cosmeticSet = rig.cosmeticSet;
            bool hasCosmetx =
                    cosmeticSet.items.Any(cosmetic => !cosmetic.isNullItem &&
                                                      !rig.concatStringOfCosmeticsAllowed.Contains(
                                                              cosmetic.itemName)) && !rig.inTryOnRoom;

            switch (hasCosmetx)
            {
                case true when !Extensions.PlayerMods[rig].Contains("[<color=red>CosmetX</color>]"):
                    Extensions.PlayerMods[rig].Add("[<color=red>CosmetX</color>]");

                    break;

                case false when Extensions.PlayerMods[rig].Contains("[<color=red>CosmetX</color>]"):
                    Extensions.PlayerMods[rig].Remove("[<color=red>CosmetX</color>]");

                    break;
            }

            if (HasNotifiedAboutCosmetX[rig] || !hasCosmetx)
                continue;

            Notifications.SendNotification(
                    $"[<color=red>Cheater</color>] Player {rig.OwningNetPlayer.SanitizedNickName} has CosmetX installed.");

            HasNotifiedAboutCosmetX[rig] = true;
        }
    }

    public static async Task<bool> IsPlayerOptedOut(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return false;

        if (OptOutCache.TryGetValue(userId, out bool cachedResult))
            return cachedResult;

        try
        {
            string              payload = JsonConvert.SerializeObject(new { user_id = userId, });
            using StringContent content = new(payload, Encoding.UTF8, "application/json");

            HttpClient                httpClient = new();
            using HttpResponseMessage response = await httpClient.PostAsync("https://api.aeris.now/opted-out", content);
            response.EnsureSuccessStatusCode();

            string         jsonString = await response.Content.ReadAsStringAsync();
            OptOutResponse result     = JsonConvert.DeserializeObject<OptOutResponse>(jsonString);

            bool skip = result is { skip: true, };
            OptOutCache[userId] = skip;

            return skip;
        }
        catch
        {
            OptOutCache[userId] = false;

            return false;
        }
    }

    private class OptOutResponse
    {
        public bool skip { get; set; }
    }
}