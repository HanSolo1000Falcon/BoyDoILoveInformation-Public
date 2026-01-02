using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoyDoILoveInformation.Core;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

namespace BoyDoILoveInformation.Tools;

public class PunCallbacks : MonoBehaviourPunCallbacks
{
    public static Dictionary<VRRig, List<string>> CheatsNotifiedAbout = new();
    private static readonly HashSet<string> CheckedPlayers = new();
    private static string currentRoom;
    public override void OnJoinedRoom()
    {
        ResetLobbyTracking();
    }

    public override void OnLeftRoom()
    {
        ResetLobbyTracking();
    }
    public override async void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        try
        {
            SyncLobbyTracking();
            string userId = targetPlayer.UserId;
    
            if (string.IsNullOrEmpty(userId))
                return;
    
            if (CheckedPlayers.Contains(userId))
                return;

            VRRig rig = GorillaParent.instance.vrrigs.Find(rig => rig.OwningNetPlayer.GetPlayerRef()
                                                                     .Equals(targetPlayer));

            if (rig == null || !rig.HasCosmetics())
                return;

            bool isPlayerOptedOut = await BDILIUtils.IsPlayerOptedOut(rig.OwningNetPlayer.UserId);

            if (isPlayerOptedOut)
                return;

            if (!Extensions.PlayerMods.ContainsKey(rig))
                Extensions.PlayerMods[rig] = [];

            Extensions.PlayerMods[rig].Clear();

            Hashtable    properties = targetPlayer.CustomProperties;
            List<string> mods       = [];
            List<string> cheats     = [];

            foreach (string key in properties.Keys)
            {
                if (Plugin.KnownCheats.TryGetValue(key, out string cheat))
                    mods.Add($"[<color=red>{cheat}</color>]");

                if (Plugin.KnownMods.TryGetValue(key, out string mod))
                    mods.Add($"[<color=green>{mod}</color>]");
            }

            foreach (string key in changedProps.Keys)
                if (Plugin.KnownCheats.TryGetValue(key, out string cheat))
                    cheats.Add($"[<color=red>{cheat}</color>]");

            CheatsNotifiedAbout.TryAdd(rig, []);
            List<string> cheatsNotNotifiedAbout = [];

            foreach (string key in cheats.Where(key => !CheatsNotifiedAbout[rig].Contains(key)))
            {
                cheatsNotNotifiedAbout.Add(key);
                CheatsNotifiedAbout[rig].Add(key);
            }

            if (cheatsNotNotifiedAbout.Count > 0)
                Notifications.SendNotification(
                        $"[<color=red>Cheater</color>] Player {rig.OwningNetPlayer.SanitizedNickName} has the following cheats: {string.Join(", ", cheatsNotNotifiedAbout)}.");

            Extensions.PlayerMods[rig] = mods;
        }
        catch
        {
            // ignored
        }
    }
    private static void SyncLobbyTracking()
    {
        string roomName = PhotonNetwork.CurrentRoom?.Name;

        if (roomName != currentRoom)
            ResetLobbyTracking();
    }

    private static void ResetLobbyTracking()
    {
        CheckedPlayers.Clear();
        currentRoom = PhotonNetwork.CurrentRoom?.Name;
    }
}
