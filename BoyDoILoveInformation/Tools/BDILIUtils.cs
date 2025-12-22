using System.Linq;
using BoyDoILoveInformation.Core;
using GorillaLocomotion;
using GorillaNetworking;
using UnityEngine;

namespace BoyDoILoveInformation.Tools;

public class BDILIUtils : MonoBehaviour
{
    public static Transform RealRightController;
    public static Transform RealLeftController;

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

        if (GorillaParent.hasInstance && GorillaParent.instance.vrrigs != null)
            foreach (VRRig rig in GorillaParent.instance.vrrigs)
            {
                if (!Extensions.PlayerMods.ContainsKey(rig))
                    Extensions.PlayerMods[rig] = [];
                
                if (!rig.HasCosmetics())
                    continue;

                CosmeticsController.CosmeticSet cosmeticSet = rig.cosmeticSet;
                bool hasCosmetx =
                        cosmeticSet.items.Any(cosmetic => !cosmetic.isNullItem &&
                                                          !rig.concatStringOfCosmeticsAllowed.Contains(
                                                                  cosmetic.itemName));

                switch (hasCosmetx)
                {
                    case true when !Extensions.PlayerMods[rig].Contains("[<color=red>CosmetX</color>]"):
                        Extensions.PlayerMods[rig].Add("[<color=red>CosmetX</color>]");
                        Notifications.SendNotification(
                                $"[<color=red>Cheater</color>] Player {rig.OwningNetPlayer.SanitizedNickName} has CosmetX installed.");
                        break;

                    case false when Extensions.PlayerMods[rig].Contains("[<color=red>CosmetX</color>]"):
                        Extensions.PlayerMods[rig].Remove("[<color=red>CosmetX</color>]");

                        break;
                }
            }
    }
}