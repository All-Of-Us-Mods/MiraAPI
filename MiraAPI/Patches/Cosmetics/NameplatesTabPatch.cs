using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AmongUs.Data;
using HarmonyLib;
using MiraAPI.Cosmetics;
using MiraAPI.Utilities.Assets.Addressable;
using MonoMod.Utils;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MiraAPI.Patches.Cosmetics;
[HarmonyPatch(typeof(NameplatesTab), nameof(NameplatesTab.OnEnable))]
public static class NameplatesTabPatch
{
    public static bool Prefix(NameplatesTab __instance)
    {
        var groups = new SortedList<AbstractCosmeticsGroup, List<NamePlateData>>(
            new AbstractCosmeticsGroupComparer()
        );
        CustomCosmeticManager.Groups.Do(x => x.Nameplates.Sort(new CosmeticComparer(x)));
        var c = CustomCosmeticManager.Groups.Select(x => new KeyValuePair<AbstractCosmeticsGroup, List<NamePlateData>>(x, x.Nameplates)).
            ToDictionary(x => x.Key, x => x.Value.Where(x => x.Free || DataManager.Player.Purchases.GetPurchase(x.ProductId, x.BundleId)).ToList());
        groups.AddRange<AbstractCosmeticsGroup, List<NamePlateData>>(c);
        Logger<MiraApiPlugin>.Info($"{c.Count}");

        foreach (var colorchip in __instance.ColorChips) colorchip.gameObject.Destroy();
        __instance.ColorChips.Clear();
        var groupNameText = __instance.GetComponentInChildren<TextMeshPro>(false);
        var hatIdx = 0;

        __instance.PlayerPreview.gameObject.SetActive(false);
        __instance.StartCoroutine(__instance.CoLoadNameplatePreview());

        foreach (var (group, hats) in groups.Where(x => x.Key.GroupVisible() && x.Key.Nameplates.Count > 0))
        {
            var text = UnityEngine.Object.Instantiate(groupNameText, __instance.scroller.Inner);
            text.gameObject.transform.localScale = Vector3.one;
            text.GetComponent<TextTranslatorTMP>().Destroy();
            text.text = group.GroupName;
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 3f;
            text.fontSizeMax = 3f;
            text.fontSizeMin = 0f;

            hatIdx = (hatIdx + 1) / 2 * 2;

            var xLerp = __instance.XRange.Lerp(0.5f);
            var yLerp = __instance.YStart - (hatIdx / __instance.NumPerRow) * __instance.YOffset;
            text.transform.localPosition = new Vector3(xLerp, yLerp, -1f);
            hatIdx += 2;

            foreach (var hat in hats)
            {
                var num = __instance.XRange.Lerp(hatIdx % __instance.NumPerRow / (__instance.NumPerRow - 1f));
                var num2 = __instance.YStart - hatIdx / __instance.NumPerRow * __instance.YOffset;

                var colorChip = UnityEngine.Object.Instantiate(__instance.ColorTabPrefab, __instance.scroller.Inner);
                colorChip.transform.localPosition = new Vector3(num, num2, -1f);

                colorChip.Button.OnClick.AddListener((Action)(() => __instance.SelectNameplate(hat)));
                colorChip.Button.ClickMask = __instance.scroller.Hitbox;

                colorChip.ProductId = hat.ProductId;

                var handle = Addressables.LoadAssetAsync<NamePlateViewData>(hat.ViewDataRef);
                Coroutines.Start(SetupColorChip(colorChip.TryCast<NameplateChip>(), handle));
                __instance.ColorChips.Add(colorChip);
                hatIdx += 1;
            }
        }

        __instance.GetDefaultSelectable().PlayerEquippedForeground.SetActive(true);
        __instance.plateId = DataManager.Player.Customization.NamePlate;
        __instance.scroller.ContentYBounds.max = -(__instance.YStart - (hatIdx + 1) / __instance.NumPerRow * __instance.YOffset) - 3f;
        __instance.currentNameplateIsEquipped = true;

        return false;
    }

    private static IEnumerator SetupColorChip(NameplateChip colorChip, AsyncOperationHandle<NamePlateViewData> handle)
    {
        yield return new WaitForAsyncOperationHandleFinish(handle);
        colorChip.image.sprite = handle.Result?.Image;
    }
}
