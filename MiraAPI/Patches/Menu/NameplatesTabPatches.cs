using AmongUs.Data;
using HarmonyLib;
using Innersloth.Assets;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches.Menu;

[HarmonyPatch(typeof(NameplatesTab))]
public static class NameplatesTabPatches
{
    private static readonly SortedList<string, List<NamePlateData>> SortedNameplates = new(new ControllableComparer<string>(["vanilla"], [], StringComparer.InvariantCulture));
    private static int currentPage;

    internal static void AddRange(IEnumerable<(string Key, NamePlateData Visor)> data)
    {
        foreach (var (key, visor) in data)
        {
            if (!SortedNameplates.ContainsKey(key)) SortedNameplates.Add(key, []);
            SortedNameplates[key].Add(visor);
        }
    }

    private static void PreviousPage(NameplatesTab tab)
    {
        currentPage--;
        currentPage = currentPage < 0 ? SortedNameplates.Count - 1 : currentPage;
        GenerateHats(tab, currentPage);
    }

    private static void NextPage(NameplatesTab tab)
    {
        currentPage++;
        currentPage = currentPage > SortedNameplates.Count - 1 ? 0 : currentPage;
        GenerateHats(tab, currentPage);
    }

    [HarmonyPatch(nameof(NameplatesTab.OnEnable))]
    [HarmonyPrefix]
    public static bool OnEnablePrefix(NameplatesTab __instance)
    {
        if (!AddressablesLoader.AddressableNameplatesExist)
        {
            return true;
        }
        __instance.plateId = HatManager.Instance.GetNamePlateById(DataManager.Player.Customization.namePlate).ProdId;

        if (!SortedNameplates.ContainsKey("Vanilla")) AddRange(HatManager.Instance.GetUnlockedNamePlates().Select(x => ("Vanilla", x)));

        InventoryUtility.CreateNextBackButtons(__instance, PreviousPage, NextPage);

        GenerateHats(__instance, currentPage);

        return false;
    }

    [HarmonyPatch(typeof(NameplatesTab), nameof(NameplatesTab.Update))]
    [HarmonyPrefix]

    public static void UpdatePrefix(NameplatesTab __instance)
    {
        if (!AddressablesLoader.AddressableNameplatesExist)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PreviousPage(__instance);
        }
        else if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            NextPage(__instance);
        }
    }

    private static void GenerateHats(NameplatesTab __instance, int page)
    {
        foreach (var instanceColorChip in __instance.ColorChips) instanceColorChip.gameObject.Destroy();
        __instance.ColorChips.Clear();
        __instance.scroller.Inner.GetComponentsInChildren<TextMeshPro>().Do(x => x.gameObject.DeepDestroy(false));

        var groupNameText = __instance.GetComponentInChildren<TextMeshPro>(false);

        int hatIndex = 0;

        var (groupName, nameplates) = SortedNameplates.ToArray()[page];
        var text = Object.Instantiate(groupNameText, __instance.scroller.Inner);
        text.enabled = true;
        text.gameObject.transform.localScale = Vector3.one;
        text.GetComponent<TextTranslatorTMP>().Destroy();
        text.EnableStencilMasking();
        text.text = $"{groupName} ({currentPage + 1}/{SortedNameplates.Count})";
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 5f;
        text.fontSizeMax = 5f;
        text.fontSizeMin = 0f;
        float xLerp = __instance.XRange.Lerp(0.5f);
        float yLerp = __instance.YStart - hatIndex / __instance.NumPerRow * __instance.YOffset;
        text.transform.localPosition = new Vector3(xLerp, yLerp, -1f);

        hatIndex += 2;
        foreach (var visor in nameplates.OrderBy(HatManager.Instance.allNamePlates.IndexOf))
        {
            float hatXPosition = __instance.XRange.Lerp(hatIndex % __instance.NumPerRow / (__instance.NumPerRow - 1f));
            float hatYPosition = __instance.YStart - hatIndex / __instance.NumPerRow * __instance.YOffset;
            GenerateColorChip(__instance, new Vector2(hatXPosition, hatYPosition), visor);
            hatIndex += 1;
        }

        __instance.SetScrollerBounds();
        __instance.currentNameplateIsEquipped = true;
    }

    private static void GenerateColorChip(NameplatesTab __instance, Vector2 position, NamePlateData namePlate)
    {
        var colorChip = Object.Instantiate(__instance.ColorTabPrefab, __instance.scroller.Inner);
        colorChip.gameObject.name = namePlate.ProductId;

        if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard)
        {
            colorChip.Button.OnClick.AddListener((Action)__instance.ClickEquip);
            colorChip.Button.OnMouseOver.AddListener((Action)(() => __instance.SelectNameplate(namePlate)));
            colorChip.Button.OnMouseOut.AddListener(
                (Action)(() => __instance.SelectNameplate(
                    HatManager.Instance.GetNamePlateById(DataManager.Player.Customization.NamePlate))));
        }
        else
        {
            colorChip.Button.OnClick.AddListener((Action)(() => __instance.SelectNameplate(namePlate)));
        }

        colorChip.Button.ClickMask = __instance.scroller.Hitbox;
        colorChip.ProductId = namePlate.ProdId;

        var x = (NamePlateViewData viewData) =>
        {
            colorChip.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = viewData?.Image;
            // (colorChip as NameplateChip).image.sprite = viewData?.Image;
        };
        __instance.StartCoroutine(AddressableAssetExtensions.CoLoadAssetAsync<NamePlateViewData>(__instance, namePlate.GetAssetReference(), x));
        colorChip.transform.localPosition = new Vector3(position.x, position.y, -1f);
        colorChip.Tag = namePlate;
        __instance.ColorChips.Add(colorChip);
    }
}
