using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AmongUs.Data;
using HarmonyLib;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches.Menu;

[HarmonyPatch(typeof(HatsTab))]
public static class HatsTabPatches
{
    private static SortedList<string, List<HatData>> sortedHats = [];
    private static readonly Dictionary<int, string> StoreNames = [];
    private static int currentPage;

    private static void PreviousPage(HatsTab hatsTab)
    {
        currentPage--;
        currentPage = currentPage < 0 ? sortedHats.Count - 1 : currentPage;
        GenerateHats(hatsTab, currentPage);
    }

    private static void NextPage(HatsTab hatsTab)
    {
        currentPage++;
        currentPage = currentPage > sortedHats.Count - 1 ? 0 : currentPage;
        GenerateHats(hatsTab, currentPage);
    }

    [HarmonyPatch(nameof(HatsTab.OnEnable))]
    [HarmonyPrefix]
    public static bool OnEnablePrefix(HatsTab __instance)
    {
        if (!AddressablesLoader.AddressableHatsExist)
        {
            return true;
        }
        __instance.currentHat = HatManager.Instance.GetHatById(DataManager.Player.Customization.Hat);
        var allHats = HatManager.Instance.GetUnlockedHats().ToImmutableList();

        if (sortedHats.Count == 0)
        {
            var num = 0;
            var comparer = new ControllableComparer<string>(["vanilla"], [], StringComparer.InvariantCulture);
            sortedHats = new SortedList<string, List<HatData>>(comparer);
            foreach (var hat in allHats)
            {
                if (!sortedHats.ContainsKey(hat.StoreName)) sortedHats[hat.StoreName] = [];
                sortedHats[hat.StoreName].Add(hat);

                if (!StoreNames.ContainsValue(hat.StoreName))
                {
                    StoreNames.Add(num, hat.StoreName);
                    num++;
                }
            }
        }

        InventoryUtility.CreateNextBackButtons(__instance, PreviousPage, NextPage);

        GenerateHats(__instance, currentPage);

        return false;
    }

    [HarmonyPatch(typeof(HatsTab), nameof(HatsTab.Update))]
    [HarmonyPrefix]
    public static void UpdatePrefix(HatsTab __instance)
    {
        if (!AddressablesLoader.AddressableHatsExist)
        {
            return;
        }
        if (sortedHats.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PreviousPage(__instance);
        }
        else if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            NextPage(__instance);
        }
    }

    private static IEnumerator? loadRoutine;
    private static int hatIndex;

    private static void GenerateHats(HatsTab __instance, int page)
    {
        if (loadRoutine != null) Coroutines.Stop(loadRoutine);

        hatIndex = 0;
        foreach (var instanceColorChip in __instance.ColorChips) instanceColorChip.gameObject.Destroy();
        __instance.ColorChips.Clear();
        __instance.scroller.Inner.GetComponentsInChildren<TextMeshPro>().Do(x => x.gameObject.DeepDestroy(false));

        var groupNameText = __instance.GetComponentInChildren<TextMeshPro>(false);
        var group = sortedHats.Where(x => x.Key == StoreNames[page]);

        foreach ((string groupName, List<HatData> hats) in group)
        {
            var text = Object.Instantiate(groupNameText, __instance.scroller.Inner);
            text.enabled = true;
            text.gameObject.transform.localScale = Vector3.one;
            text.GetComponent<TextTranslatorTMP>().Destroy();
            text.EnableStencilMasking();
            text.text = $"{groupName} ({currentPage + 1}/{sortedHats.Count})";
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 5f;
            text.fontSizeMax = 5f;
            text.fontSizeMin = 0f;
            var xLerp = __instance.XRange.Lerp(0.5f);
            var yLerp = __instance.YStart - hatIndex / __instance.NumPerRow * __instance.YOffset;
            text.transform.localPosition = new Vector3(xLerp, yLerp, -1f);

            hatIndex += 5;
            loadRoutine = Coroutines.Start(CoGenerateChips(__instance, hats));
        }
        __instance.scroller.ContentYBounds.max = -(__instance.YStart - (hatIndex + 1) / __instance.NumPerRow * __instance.YOffset) - 3f;
        __instance.currentHatIsEquipped = true;
    }

    private static IEnumerator CoGenerateChips(HatsTab __instance, List<HatData> hats)
    {
        __instance.scroller.ScrollToTop();
        var batchSize = 5;
        for (var i = 0; i < hats.Count; i += batchSize)
        {
            var batch = hats.Skip(i).Take(batchSize).ToList();

            foreach (var hat in batch.OrderBy(HatManager.Instance.allHats.IndexOf))
            {
                var hatXPosition = __instance.XRange.Lerp(hatIndex % __instance.NumPerRow / (__instance.NumPerRow - 1f));
                var hatYPosition = __instance.YStart - hatIndex / __instance.NumPerRow * __instance.YOffset;
                GenerateColorChip(__instance, new Vector2(hatXPosition, hatYPosition), hat);
                hatIndex += 1;
                yield return null;
            }

            __instance.SetScrollerBounds();
            yield return new WaitForSeconds(0.01f);
        }
        __instance.currentHatIsEquipped = true;
        loadRoutine = null;
    }

    private static void GenerateColorChip(HatsTab __instance, Vector2 position, HatData hat)
    {
        var colorChip = Object.Instantiate(__instance.ColorTabPrefab, __instance.scroller.Inner);
        colorChip.gameObject.name = hat.ProductId;
        if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard)
        {
            colorChip.Button.OnClick.AddListener((Action)__instance.ClickEquip);
            colorChip.Button.OnMouseOver.AddListener((Action)(() => __instance.SelectHat(hat)));
            colorChip.Button.OnMouseOut.AddListener((Action)(() => __instance.SelectHat(HatManager.Instance.GetHatById(DataManager.Player.Customization.Hat))));
        }
        else
        {
            colorChip.Button.OnClick.AddListener((Action)(() => __instance.SelectHat(hat)));
        }

        colorChip.Inner.SetHat(hat, __instance.HasLocalPlayer() ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color);
        colorChip.Button.ClickMask = __instance.scroller.Hitbox;
        colorChip.SelectionHighlight.gameObject.SetActive(false);
        __instance.UpdateMaterials(colorChip.Inner.FrontLayer, hat);
        colorChip.Inner.SetMaskType(PlayerMaterial.MaskType.SimpleUI);
        colorChip.transform.localPosition = new Vector3(position.x, position.y, -1f);
        colorChip.Inner.transform.localPosition = hat.ChipOffset + new Vector2(0f, -0.3f);
        colorChip.Tag = hat;
        __instance.ColorChips.Add(colorChip);
    }
}
