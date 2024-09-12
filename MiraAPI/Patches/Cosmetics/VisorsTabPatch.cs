using AmongUs.Data;
using HarmonyLib;
using MiraAPI.Cosmetics;
using MonoMod.Utils;
using Reactor.Utilities.Extensions;
using Reactor.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.AddressableAssets;
using UnityEngine;
using MiraAPI.Utilities.Assets.Addressable;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;

namespace MiraAPI.Patches.Cosmetics;
[HarmonyPatch(typeof(VisorsTab), nameof(VisorsTab.OnEnable))]
public static class VisorsTabPatch
{
    public static bool Prefix(VisorsTab __instance)
    {
        var Groups = new SortedList<AbstractCosmeticsGroup, List<VisorData>>(
            new AbstractCosmeticsGroupComparer()
        );
        CustomCosmeticManager.Groups.Do(x => x.Visors.Sort(new CosmeticComparer(x)));
        var c = CustomCosmeticManager.Groups.Select(x => new KeyValuePair<AbstractCosmeticsGroup, List<VisorData>>(x, x.Visors)).
            ToDictionary(x => x.Key, x => x.Value.Where(x => x.Free || DataManager.Player.Purchases.GetPurchase(x.ProductId, x.BundleId)).ToList());
        Groups.AddRange<AbstractCosmeticsGroup, List<VisorData>>(c);

        foreach (var colorchip in __instance.ColorChips) colorchip.gameObject.Destroy();
        __instance.ColorChips.Clear();
        var groupNameText = __instance.GetComponentInChildren<TextMeshPro>(false);
        int hatIdx = 0;

        foreach (var (group, visors) in Groups.Where(x => x.Key.GroupVisible() && x.Key.Visors.Count > 0))
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

            foreach (var visor in visors)
            {
                float num = __instance.XRange.Lerp(hatIdx % __instance.NumPerRow / (__instance.NumPerRow - 1f));
                float num2 = __instance.YStart - hatIdx / __instance.NumPerRow * __instance.YOffset;
                ColorChip colorChip = UnityEngine.Object.Instantiate(__instance.ColorTabPrefab, __instance.scroller.Inner);
                colorChip.transform.localPosition = new Vector3(num, num2, -1f);
                colorChip.Button.OnClick.AddListener((Action)(() => __instance.SelectVisor(visor)));
                colorChip.Button.ClickMask = __instance.scroller.Hitbox;
                colorChip.ProductId = visor.ProductId;
                __instance.UpdateMaterials(colorChip.Inner.FrontLayer, visor);

                var handle = Addressables.LoadAssetAsync<VisorViewData>(visor.ViewDataRef);
                Coroutines.Start(SetupColorChip(handle, __instance.HasLocalPlayer() ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color, colorChip));
                colorChip.SelectionHighlight.gameObject.SetActive(false);
                __instance.ColorChips.Add(colorChip);
                hatIdx += 1;
            }
        }

        __instance.visorId = DataManager.Player.Customization.Visor;
        __instance.currentVisorIsEquipped = true;
        __instance.scroller.ContentYBounds.max = -(__instance.YStart - (hatIdx + 1) / __instance.NumPerRow * __instance.YOffset) - 3f;

        return false;
    }

    internal static IEnumerator SetupColorChip(AsyncOperationHandle<VisorViewData> handle, int colorId, ColorChip colorChip)
    {
        yield return new WaitForAsyncOperationHandleFinish(handle);
        colorChip.Inner.FrontLayer.sprite = handle.Result?.IdleFrame;
        PlayerMaterial.SetColors(colorId, colorChip.Inner.FrontLayer);
    }
}
