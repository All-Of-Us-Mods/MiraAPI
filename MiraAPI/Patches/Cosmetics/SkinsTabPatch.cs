﻿using System;
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
[HarmonyPatch(typeof(SkinsTab), nameof(SkinsTab.OnEnable))]
public static class SkinsTabPatch
{
    public static bool Prefix(SkinsTab __instance)
    {
        var groups = new SortedList<AbstractCosmeticsGroup, List<SkinData>>(
            new AbstractCosmeticsGroupComparer()
        );
        CustomCosmeticManager.Groups.Do(x => x.Skins.Sort(new CosmeticComparer(x)));
        var c = CustomCosmeticManager.Groups.Select(x => new KeyValuePair<AbstractCosmeticsGroup, List<SkinData>>(x, x.Skins)).
            ToDictionary(x => x.Key, x => x.Value.Where(x => x.Free || DataManager.Player.Purchases.GetPurchase(x.ProductId, x.BundleId)).ToList());
        groups.AddRange<AbstractCosmeticsGroup, List<SkinData>>(c);

        foreach (var colorchip in __instance.ColorChips) colorchip.gameObject.Destroy();
        __instance.ColorChips.Clear();
        var groupNameText = __instance.GetComponentInChildren<TextMeshPro>(false);
        var hatIdx = 0;

        foreach (var (group, skins) in groups.Where(x => x.Key.GroupVisible() && x.Key.Skins.Count > 0))
        {
            var text = UnityEngine.Object.Instantiate(groupNameText, __instance.scroller.Inner);
            text.gameObject.transform.localScale = Vector3.one;
            text.GetComponent<TextTranslatorTMP>().Destroy();
            text.text = group.GroupName;
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 3f;
            text.fontSizeMax = 3f;
            text.fontSizeMin = 0f;

            hatIdx = (hatIdx + 4) / 5 * 5;

            var xLerp = __instance.XRange.Lerp(0.5f);
            var yLerp = __instance.YStart - (hatIdx / __instance.NumPerRow) * __instance.YOffset;
            text.transform.localPosition = new Vector3(xLerp, yLerp, -1f);
            hatIdx += 5;

            foreach (var skin in skins)
            {
                var num = __instance.XRange.Lerp(hatIdx % __instance.NumPerRow / (__instance.NumPerRow - 1f));
                var num2 = __instance.YStart - hatIdx / __instance.NumPerRow * __instance.YOffset;
                var colorChip = UnityEngine.Object.Instantiate(__instance.ColorTabPrefab, __instance.scroller.Inner);
                colorChip.transform.localPosition = new Vector3(num, num2, -1f);
                colorChip.Button.OnClick.AddListener((Action)(() => __instance.SelectSkin(skin)));
                colorChip.Button.ClickMask = __instance.scroller.Hitbox;
                colorChip.ProductId = skin.ProductId;
                __instance.UpdateMaterials(colorChip.Inner.FrontLayer, skin);

                var handle = Addressables.LoadAssetAsync<SkinViewData>(skin.ViewDataRef);
                Coroutines.Start(SetupColorChip(handle, __instance.HasLocalPlayer() ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color, colorChip));
                colorChip.SelectionHighlight.gameObject.SetActive(false);
                __instance.ColorChips.Add(colorChip);
                hatIdx += 1;
            }
        }

        __instance.skinId = DataManager.Player.Customization.Skin;
        __instance.currentSkinIsEquipped = true;
        __instance.scroller.ContentYBounds.max = -(__instance.YStart - (hatIdx + 1) / __instance.NumPerRow * __instance.YOffset) - 3f;

        return false;
    }

    internal static IEnumerator SetupColorChip(AsyncOperationHandle<SkinViewData> handle, int colorId, ColorChip colorChip)
    {
        yield return new WaitForAsyncOperationHandleFinish(handle);
        colorChip.Inner.FrontLayer.sprite = handle.Result?.IdleFrame;
        PlayerMaterial.SetColors(colorId, colorChip.Inner.FrontLayer);
    }
}