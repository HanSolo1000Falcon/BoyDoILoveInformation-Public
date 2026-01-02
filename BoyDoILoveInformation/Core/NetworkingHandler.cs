using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BoyDoILoveInformation.Tools;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace BoyDoILoveInformation.Core;

public class NetworkingHandler : MonoBehaviour
{
    private const byte NetworkingByte = 135;

    private static readonly Vector3    NetworkedMenuLocalPosition = new(-0.2487f, 0.0197f, 0f);
    private static readonly Quaternion NetworkedMenuLocalRotation = Quaternion.Euler(331.9757f, 348.8297f, 22.7959f);
    
    public static Action<bool> OnMenuStateUpdate;

    private GameObject emptyMenuPrefab;

    private Dictionary<VRRig, GameObject> playerMenus    = new();
    private Dictionary<VRRig, float>      lastTimeOpened = new();

    private void Start()
    {
        emptyMenuPrefab                      = Plugin.BDILIBundle.LoadAsset<GameObject>("EmptyMenu");
        emptyMenuPrefab.transform.localScale = MenuHandler.TargetMenuScale;
        MenuHandler.PerformShaderManagement(emptyMenuPrefab);
        
        BDILIUtils.OnPlayerCosmeticsLoaded += rig =>
                                              {
                                                  playerMenus[rig] = Instantiate(emptyMenuPrefab, rig.leftHandTransform);
                                                  playerMenus[rig].SetActive(false);
                                                  playerMenus[rig].transform.localPosition = NetworkedMenuLocalPosition;
                                                  playerMenus[rig].transform.localRotation = NetworkedMenuLocalRotation;
                                                  lastTimeOpened[rig] = Time.time;
                                              };
        BDILIUtils.OnPlayerRigCached       += rig =>
                                              {
                                                  if (!playerMenus.TryGetValue(rig, out GameObject menu))
                                                      return;
                                                  
                                                  Destroy(menu);
                                                  playerMenus.Remove(rig);
                                                  lastTimeOpened.Remove(rig);
                                              };

        OnMenuStateUpdate += open => PhotonNetwork.RaiseEvent(NetworkingByte, open,
                                     new RaiseEventOptions() { Receivers = ReceiverGroup.Others, }, SendOptions.SendReliable);

        PhotonNetwork.NetworkingClient.EventReceived += eventData =>
                                                        {
                                                            if (eventData.Code != NetworkingByte)
                                                                return;
                                                            
                                                            VRRig sender = GorillaParent.instance.vrrigs
                                                                   .FirstOrDefault(rig => rig.OwningNetPlayer.ActorNumber ==
                                                                        eventData.Sender);

                                                            if (sender == null || !playerMenus.ContainsKey(sender) || !eventData.Parameters.TryGetValue(ParameterCode.Data, out object data) || data is not bool menuEnabled || lastTimeOpened[sender] + 0.05f > Time.time)
                                                                return;
                                                            
                                                            lastTimeOpened[sender] = Time.time;
                                                            playerMenus[sender].SetActive(menuEnabled);
                                                        };
    }
}