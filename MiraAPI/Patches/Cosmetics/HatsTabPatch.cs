using AmongUs.Data;
using HarmonyLib;
using MiraAPI.Cosmetics;
using MonoMod.Utils;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace MiraAPI.Patches.Cosmetics;

[HarmonyPatch(typeof(HatsTab), nameof(HatsTab.OnEnable))]
public static class HatsTabPatch
{
    public static bool Prefix(HatsTab __instance)
    {
        var hatGroups = new SortedList<AbstractCosmeticsGroup, List<HatData>>(
            new AbstractCosmeticsGroupComparer()
        );
        CustomCosmeticManager.Groups.Do(x => x.Hats.Sort(new CosmeticComparer(x)));
        var c = CustomCosmeticManager.Groups.Select(x => new KeyValuePair<AbstractCosmeticsGroup, List<HatData>>(x, x.Hats)).
            ToDictionary(x => x.Key, x => x.Value.Where(x=>x.Free || DataManager.Player.Purchases.GetPurchase(x.ProductId, x.BundleId)).ToList());
        hatGroups.AddRange<AbstractCosmeticsGroup, List<HatData>>(c);

        foreach (var colorchip in __instance.ColorChips) colorchip.gameObject.Destroy();
        __instance.ColorChips.Clear();
        var groupNameText = __instance.GetComponentInChildren<TextMeshPro>(false);
        int hatIdx = 0;

        __instance.currentHat = DestroyableSingleton<HatManager>.Instance.GetHatById(DataManager.Player.Customization.Hat);

        foreach (var (group,hats) in hatGroups.Where(x=>x.Key.GroupVisible() && x.Key.Hats.Count > 0))
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

            float xLerp = __instance.XRange.Lerp(0.5f);
            float yLerp = __instance.YStart - (hatIdx / __instance.NumPerRow) * __instance.YOffset;
            text.transform.localPosition = new Vector3(xLerp, yLerp, -1f);
            hatIdx += 5;

            foreach (var hat in hats)
            {
                float num = __instance.XRange.Lerp(hatIdx % __instance.NumPerRow / (__instance.NumPerRow - 1f));
                float num2 = __instance.YStart - hatIdx / __instance.NumPerRow * __instance.YOffset;
                ColorChip colorChip = UnityEngine.Object.Instantiate(__instance.ColorTabPrefab, __instance.scroller.Inner);
                colorChip.transform.localPosition = new Vector3(num, num2, -1f);
                colorChip.Button.OnClick.AddListener((Action)(() => __instance.SelectHat(hat)));
                colorChip.Button.ClickMask = __instance.scroller.Hitbox;
                colorChip.Inner.SetMaskType(PlayerMaterial.MaskType.SimpleUI);
                __instance.UpdateMaterials(colorChip.Inner.FrontLayer, hat);
                colorChip.Inner.SetHat(hat, __instance.HasLocalPlayer() ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color);
                colorChip.Inner.transform.localPosition = hat.ChipOffset + new Vector2(0f, -0.3f);
                colorChip.Tag = hat;
                colorChip.SelectionHighlight.gameObject.SetActive(false);
                __instance.ColorChips.Add(colorChip);
                hatIdx += 1;
            }
        }

        __instance.scroller.ContentYBounds.max = -(__instance.YStart - (hatIdx + 1) / __instance.NumPerRow * __instance.YOffset) - 3f;
        __instance.currentHatIsEquipped = true;

        return false;
    }
}
