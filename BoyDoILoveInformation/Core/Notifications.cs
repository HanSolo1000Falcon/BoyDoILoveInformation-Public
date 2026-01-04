using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BoyDoILoveInformation.Tools;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.UI;

namespace BoyDoILoveInformation.Core;

public class Notifications : MonoBehaviour
{
    private const int MaxNotifications = 15;

    private static Notifications Instance;

    private readonly List<(Guid Id, string Message)> notifications = new();
    private          GameObject                      canvas;

    private Text notificationText;

    private void Awake() => Instance = this;

    private void Start()
    {
        GameObject canvasPrefab = Plugin.BDILIBundle.LoadAsset<GameObject>("Notifications");
        canvas = Instantiate(canvasPrefab, GTPlayer.Instance.headCollider.transform);
        Destroy(canvasPrefab);
        canvas.name = "Notifications";

        canvas.transform.localPosition = new Vector3(0.1f, 0.2f, 0.6f);

        canvas.transform.localRotation = Quaternion.Euler(345f, 0f, 0f);

        notificationText = canvas.GetComponentInChildren<Text>();
        canvas.SetLayer(UnityLayer.FirstPersonOnly);
        ApplyNotificationText();
    }

    public static void SendNotification(string message)
    {
        Guid notificationId = Guid.NewGuid();
        message                                = message.InsertNewlinesWithRichText(40);
        Instance.notifications.Add((notificationId, message));
        if (Instance.notifications.Count > MaxNotifications)
        {
            Instance.notifications.RemoveAt(0);
        }

        Instance.ApplyNotificationText();
        Instance.StartCoroutine(Instance.RemoveNotificationAfterTime(notificationId));
    }

    private IEnumerator RemoveNotificationAfterTime(Guid notificationId)
    {
        yield return new WaitForSeconds(10f);
        int removed = notifications.RemoveAll(notification => notification.Id == notificationId);
        if (removed > 0)
        {
            ApplyNotificationText();
        }
    }

    private void ApplyNotificationText()
    {
        const int BaseSize = 32;
        const int MinSize  = 16;
        const int Step     = 4;

        string[] ordered = notifications.Select(notification => notification.Message).ToArray();
        string   text    = string.Empty;

        for (int i = ordered.Length - 1; i > -1; i--)
        {
            int size = Mathf.Max(MinSize, BaseSize - Step * (ordered.Length - i - 1));
            text += $"<size={size}>{ordered[i]}</size>\n\n";
        }

        notificationText.supportRichText = true;
        notificationText.text            = text.Trim();
    }
}
