using System.Linq;
using BoyDoILoveInformation.Tools;
using TMPro;

namespace BoyDoILoveInformation.Tab_Handlers;

public class ModsHandler : TabHandlerBase
{
    private TextMeshPro installedMods;
    private TextMeshPro playerName;

    private void Start()
    {
        playerName    = transform.GetChild(0).GetComponent<TextMeshPro>();
        installedMods = transform.GetChild(1).GetComponent<TextMeshPro>();

        string special = "";

        if (InformationHandler.ChosenRig != null)
            special = Plugin.HanSoloPlayerIDs.Contains(InformationHandler.ChosenRig.OwningNetPlayer.UserId)
                              ? " : <color=yellow>HanSolo1000Falcon</color>"
                              : "";

        playerName.text = InformationHandler.ChosenRig == null
                                  ? "No player selected"
                                  : InformationHandler.ChosenRig.OwningNetPlayer.SanitizedNickName + special;

        installedMods.text = InformationHandler.ChosenRig == null
                                     ? "-"
                                     : InformationHandler.ChosenRig.GetPlayerMods()        != null ||
                                       InformationHandler.ChosenRig.GetPlayerMods().Length > 0
                                             ? InformationHandler.ChosenRig.GetPlayerMods().Join("\n")
                                             : "No mods detected";
    }

    private void OnEnable()
    {
        if (playerName == null || installedMods == null)
            return;

        string special = "";

        if (InformationHandler.ChosenRig != null)
            special = Plugin.HanSoloPlayerIDs.Contains(InformationHandler.ChosenRig.OwningNetPlayer.UserId)
                              ? " : <color=yellow>HanSolo1000Falcon</color>"
                              : "";

        playerName.text = InformationHandler.ChosenRig == null
                                  ? "No player selected"
                                  : InformationHandler.ChosenRig.OwningNetPlayer.SanitizedNickName + special;

        installedMods.text = InformationHandler.ChosenRig == null
                                     ? "-"
                                     : InformationHandler.ChosenRig.GetPlayerMods()        != null ||
                                       InformationHandler.ChosenRig.GetPlayerMods().Length > 0
                                             ? InformationHandler.ChosenRig.GetPlayerMods().Join("\n")
                                             : "No mods detected";
    }
}