using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoyDoILoveInformation.Core;
using BoyDoILoveInformation.Tools;
using ExitGames.Client.Photon;
using HarmonyLib;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace BoyDoILoveInformation.Patches;

[HarmonyPatch(typeof(VRRig))]
[HarmonyPatch("IUserCosmeticsCallback.OnGetUserCosmetics", MethodType.Normal)]
public static class PlayerCosmeticsLoadedPatch
{
    private static readonly DateTime OculusPayDay = new(2023, 02, 06);
    private static          void     Postfix(VRRig __instance) => OnLoad(__instance);

    private static async void OnLoad(VRRig rig)
    {
        bool isPlayerOptedOut = await BDILIUtils.IsPlayerOptedOut(rig.OwningNetPlayer.UserId);

        Extensions.PlayersWithCosmetics.Add(rig);

        DateTime playerCreationDate;

        if (!Extensions.AccountCreationDates.TryGetValue(rig.OwningNetPlayer.UserId, out playerCreationDate))
        {
            TaskCompletionSource<GetAccountInfoResult> tcs = new();

            PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest { PlayFabId = rig.OwningNetPlayer.UserId, },
                    result => tcs.SetResult(result),
                    error =>
                    {
                        Debug.LogError("Failed to get account info: " + error.ErrorMessage);
                        tcs.SetException(new Exception(error.ErrorMessage));
                    });

            GetAccountInfoResult result = await tcs.Task;
            Extensions.AccountCreationDates[rig.OwningNetPlayer.UserId] = result.AccountInfo.Created;
            playerCreationDate                                          = result.AccountInfo.Created;
        }

        if (!isPlayerOptedOut)
        {
            Hashtable    properties = rig.OwningNetPlayer.GetPlayerRef().CustomProperties;
            List<string> mods       = [];
            List<string> cheats     = [];

            foreach (string key in properties.Keys)
            {
                if (Plugin.KnownCheats.TryGetValue(key, out string cheat))
                {
                    mods.Add($"[<color=red>{cheat}</color>]");
                    cheats.Add(cheat);
                }

                if (Plugin.KnownMods.TryGetValue(key, out string mod))
                    mods.Add($"[<color=green>{mod}</color>]");
            }

            if (cheats.Count > 0)
                Notifications.SendNotification(
                        $"[<color=red>Cheater</color>] Player {rig.OwningNetPlayer.SanitizedNickName} has the following cheats: {string.Join(", ", cheats)}.");

            Extensions.PlayerMods[rig] = mods;
        }

        BDILIUtils.OnPlayerCosmeticsLoaded?.Invoke(rig);

        string cosmeticsAllowed = rig.concatStringOfCosmeticsAllowed.ToLower();

        if (cosmeticsAllowed.Contains("s. first login"))
        {
            Extensions.PlayerPlatforms[rig] = GamePlatform.Steam;

            return;
        }

        if (cosmeticsAllowed.Contains("first login") || cosmeticsAllowed.Contains("game-purchase"))
        {
            Extensions.PlayerPlatforms[rig] = GamePlatform.OculusPC;

            return;
        }

        if (rig.OwningNetPlayer.GetPlayerRef().CustomProperties.Count > 1)
        {
            Extensions.PlayerPlatforms[rig] = GamePlatform.PC;

            return;
        }

        if (playerCreationDate > OculusPayDay)
        {
            Extensions.PlayerPlatforms[rig] = GamePlatform.Standalone;

            return;
        }

        Extensions.PlayerPlatforms[rig] = GamePlatform.Unknown;
    }
}