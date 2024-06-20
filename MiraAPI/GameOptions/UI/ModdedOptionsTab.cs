using System;
using System.Collections.Generic;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace MiraAPI.GameOptions.UI;
public static class ModdedOptionsTab
{
    public static readonly List<GameObject> CustomTabs = [];
    public static readonly List<GameObject> CustomScreens = [];

    public static GameObject InitializeForMod(IMiraConfig config, GameSettingMenu __instance)
    {
        var gameBtn = __instance.transform.FindChild("Header/Tabs/GameTab").gameObject;
        var roleBtn = __instance.transform.FindChild("Header/Tabs/RoleTab").gameObject;
        var screen = CreateNewMenu(config, __instance);
        var tab = CreateCustomTab(config, __instance, screen, gameBtn, roleBtn);

        CustomScreens.Add(screen);
        CustomTabs.Add(tab);

        SpriteRenderer rend = tab.transform.FindChild("Btn/Tab Background").GetComponent<SpriteRenderer>();
        rend.enabled = false;

        UpdateListeners(__instance, gameBtn.GetComponentInChildren<PassiveButton>(), roleBtn.GetComponentInChildren<PassiveButton>(), rend);

        var container = screen.transform.FindChild("GameGroup/SliderInner");
        container.DestroyChildren();
        //CreateNewResetButton(__instance, container);

        return container.gameObject;
    }

    private static void UpdateListeners(GameSettingMenu __instance, PassiveButton gameB, PassiveButton roleB, SpriteRenderer rend)
    {
        gameB.OnClick.RemoveAllListeners();
        gameB.OnClick.AddListener((UnityAction)(() =>
        {
            __instance.RegularGameSettings.SetActive(true);
            __instance.RolesSettings.gameObject.SetActive(false);

            foreach (var screen in CustomScreens) screen.gameObject.SetActive(false);

            rend.enabled = false;

            __instance.GameSettingsHightlight.enabled = true;
            __instance.RolesSettingsHightlight.enabled = false;
        }));

        roleB.OnClick.RemoveAllListeners();
        roleB.OnClick.AddListener((UnityAction)(() =>
        {
            __instance.RegularGameSettings.SetActive(false);
            __instance.RolesSettings.gameObject.SetActive(true);

            foreach (var screen in CustomScreens) screen.gameObject.SetActive(false);

            rend.enabled = false;

            __instance.GameSettingsHightlight.enabled = false;
            __instance.RolesSettingsHightlight.enabled = true;
        }));
    }

    public static GameObject CreateHeader(ToggleOption toggleOpt, Transform container, string title, Color color)
    {
        var header = Object.Instantiate(toggleOpt, container);

        header.Title = StringNames.None;
        header.TitleText.text = title;
        header.TitleText.color = color;
        header.name = "Header";

        var checkBox = header.transform.FindChild("CheckBox")?.gameObject;
        if (checkBox)
        {
            checkBox.Destroy();
        }

        var background = header.transform.FindChild("Background")?.gameObject;
        if (background)
        {
            background.Destroy();
        }

        header.GetComponent<OptionBehaviour>().Destroy();
        return header.gameObject;
    }

    public static GameObject CreateCustomTab(IMiraConfig options, GameSettingMenu __instance, GameObject newSettings,
        GameObject gameTab, GameObject roleTab)
    {
        var newTab = Object.Instantiate(gameTab, gameTab.transform.parent);
        newTab.name = "Tab";
        newTab.transform.position = roleTab.transform.position;
        newTab.transform.position += new Vector3(CustomTabs.Count + 1, 0, 0);
        __instance.Tabs.transform.position -= new Vector3(0.5f, 0, 0);

        var inside = newTab.transform.FindChild("ColorButton");
        inside.name = "Btn";

        var btn = inside.GetComponentInChildren<PassiveButton>();

        btn.OnClick.RemoveAllListeners();

        btn.OnClick.AddListener((Action)TabAction);

        var spriteRend = inside.GetComponentInChildren<SpriteRenderer>();
        // if (options.TabSettings.TabIcon != null) spriteRend.sprite = options.TabSettings.TabIcon.LoadAsset();

        return newTab;

        void TabAction()
        {
            __instance.RegularGameSettings.SetActive(false);
            __instance.RolesSettings.gameObject.SetActive(false);

            foreach (var screen in CustomScreens)
            {
                screen.gameObject.SetActive(false);
            }
            newSettings.gameObject.SetActive(true);

            SpriteRenderer rend = newTab.transform.FindChild("Btn/Tab Background").GetComponent<SpriteRenderer>();
            rend.enabled = true;

            gameTab.transform.FindChild("ColorButton/Tab Background").gameObject.GetComponent<SpriteRenderer>().enabled = false;
            roleTab.transform.FindChild("Hat Button/Tab Background").gameObject.GetComponent<SpriteRenderer>().enabled = false;

            foreach (var tab in CustomTabs)
            {
                tab.transform.FindChild("Btn/Tab Background").gameObject.GetComponent<SpriteRenderer>().enabled = false;
            }
        }
    }

    public static GameObject CreateNewMenu(IMiraConfig config, GameSettingMenu __instance)
    {
        var gameSettings = __instance.RegularGameSettings;
        var newSettings = Object.Instantiate(gameSettings, gameSettings.transform.parent);
        newSettings.name = "Settings";
        newSettings.SetActive(false);

        var launchpadGroup = newSettings.transform.FindChild("GameGroup").gameObject;
        var text = launchpadGroup.transform.FindChild("Text").gameObject.GetComponent<TextMeshPro>();
        text.gameObject.GetComponent<TextTranslatorTMP>().Destroy();
        text.text = $"{config.TabSettings.Title} Options";

        return newSettings;
    }
}