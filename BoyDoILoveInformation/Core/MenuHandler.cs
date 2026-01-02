using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using BoyDoILoveInformation.Tab_Handlers;
using BoyDoILoveInformation.Tools;
using TMPro;
using UnityEngine;

namespace BoyDoILoveInformation.Core;

public class MenuHandler : MonoBehaviour
{
    public static readonly Vector3 TargetMenuScale = Vector3.one * 15f;

    public static readonly Vector3    BaseMenuPosition = new(0.25f, 0f, 0.05f);
    public static readonly Quaternion BaseMenuRotation = Quaternion.Euler(300f, 0f, 180f);

    public static GameObject Menu;

    public bool IsMenuOpen;

    private bool wasPressed;

    private void Start()
    {
        GameObject menuPrefab = Plugin.BDILIBundle.LoadAsset<GameObject>("Menu");
        Menu = Instantiate(menuPrefab, BDILIUtils.RealLeftController);
        Destroy(menuPrefab);
        Menu.name = "Menu";

        Menu.transform.localPosition = BaseMenuPosition;
        Menu.transform.localRotation = BaseMenuRotation;
        Menu.transform.localScale    = TargetMenuScale;

        Plugin.MainColour      = Menu.GetComponent<Renderer>().material.color;
        Plugin.SecondaryColour = Menu.transform.GetChild(0).GetComponent<Renderer>().material.color;
        Menu.SetActive(false);

        PerformShaderManagement(Menu);
        SetUpTabs();
    }

    private void Update()
    {
        bool isPressed = Plugin.MenuOpenButton.Value switch
                         {
                                 ButtonType.LeftSecondary => ControllerInputPoller.instance
                                        .leftControllerSecondaryButton,
                                 ButtonType.RightSecondary => ControllerInputPoller.instance
                                        .rightControllerSecondaryButton,
                                 ButtonType.LeftPrimary  => ControllerInputPoller.instance.leftControllerPrimaryButton,
                                 ButtonType.RightPrimary => ControllerInputPoller.instance.rightControllerPrimaryButton,
                         };

        if (isPressed && !wasPressed)
        {
            IsMenuOpen = !IsMenuOpen;
            StartCoroutine(IsMenuOpen ? OpenMenu() : CloseMenu());
            InformationHandler.ChosenRig = null;
            NetworkingHandler.OnMenuStateUpdate?.Invoke(IsMenuOpen);
        }

        wasPressed = isPressed;
    }

    public static void PerformShaderManagement(GameObject obj)
    {
        foreach (Transform child in obj.transform)
            PerformShaderManagement(child.gameObject);

        if (obj.TryGetComponent(out Renderer renderer))
            if (renderer.material.shader.name.Contains("Universal"))
            {
                renderer.material.shader = Plugin.UberShader;
                if (renderer.material.mainTexture != null) renderer.material.EnableKeyword("_USE_TEXTURE");
            }

        if (obj.TryGetComponent(out TextMeshPro tmp))
            tmp.fontMaterial = new Material(tmp.fontMaterial)
            {
                    shader = Shader.Find("TextMeshPro/Mobile/Distance Field"),
            };

        if (obj.TryGetComponent(out TextMeshProUGUI tmpUGUI))
            tmpUGUI.fontMaterial = new Material(tmpUGUI.fontMaterial)
            {
                    shader = Shader.Find("TextMeshPro/Mobile/Distance Field"),
            };
    }

    private void SetUpTabs()
    {
        Type[] tabHandlerTypes = Assembly.GetExecutingAssembly().GetTypes()
                                         .Where(t => !t.IsAbstract && t.IsClass &&
                                                     typeof(TabHandlerBase).IsAssignableFrom(t)).ToArray();

        Transform[] tabViews = Menu.transform.GetChild(1).GetComponentsInChildren<Transform>(true)
                                   .Where(t => t.parent == Menu.transform.GetChild(1) && t.name.EndsWith("Tab"))
                                   .ToArray();

        Transform[] tabButtons = Menu.transform.GetChild(0).GetComponentsInChildren<Transform>(true)
                                     .Where(t => t.parent == Menu.transform.GetChild(0) && t.name.EndsWith("TabButton"))
                                     .ToArray();

        foreach (Type tabHandlerType in tabHandlerTypes)
        {
            string    tabName   = tabHandlerType.Name.Replace("Handler", "");
            Transform tabView   = tabViews.FirstOrDefault(t => t.gameObject.name   == tabName + "Tab");
            Transform tabButton = tabButtons.FirstOrDefault(t => t.gameObject.name == tabName + "TabButton");

            if (tabView == null || tabButton == null)
            {
                Debug.LogWarning($"[MenuHandler] Could not find tab or button for tab: {tabName}");

                continue;
            }

            BDILIButton button = tabButton.gameObject.AddComponent<BDILIButton>();
            button.OnPress = () =>
                             {
                                 foreach (Transform tab in tabViews)
                                     tab.gameObject.SetActive(false);

                                 tabView.gameObject.SetActive(true);
                             };

            tabView.gameObject.AddComponent(tabHandlerType);
            tabView.gameObject.SetActive(false);
        }
    }

    private IEnumerator OpenMenu()
    {
        Menu.SetActive(true);
        Menu.transform.localScale = Vector3.zero;
        float startTime = Time.time;

        while (Time.time - startTime < 0.1f)
        {
            float t = (Time.time - startTime) / 0.1f;
            Menu.transform.localScale = Vector3.Lerp(Vector3.zero, TargetMenuScale, t);

            yield return null;
        }

        Menu.transform.localScale = TargetMenuScale;
    }

    private IEnumerator CloseMenu()
    {
        Menu.transform.localScale = TargetMenuScale;
        float startTime = Time.time;

        while (Time.time - startTime < 0.1f)
        {
            float t = (Time.time - startTime) / 0.1f;
            Menu.transform.localScale = Vector3.Lerp(TargetMenuScale, Vector3.zero, t);

            yield return null;
        }

        Menu.transform.localScale = Vector3.zero;
        Menu.SetActive(false);
    }
}