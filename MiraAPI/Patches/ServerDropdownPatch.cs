using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace MiraAPI.Patches;

[HarmonyPatch(typeof(ServerDropdown), nameof(ServerDropdown.FillServerOptions))]
public static class ServerDropdownPatch
{
    public static bool RegionEquals(this IRegionInfo region, IRegionInfo other)
    {
        return region.Name == other.Name &&
               region.TranslateName == other.TranslateName &&
               region.PingServer == other.PingServer &&
               region.TargetServer == other.TargetServer &&
               region.Servers.All(s => other.Servers.Any(x => x.Equals(s)));
    }

    public static bool Prefix(ServerDropdown __instance)
    {
        var num = 0;
        __instance.background.size = new Vector2(8.4f, 4.8f);

        foreach (var regionInfo in ServerManager.Instance.AvailableRegions)
        {
            var findingGame = SceneManager.GetActiveScene().name is "FindAGame";

            if (ServerManager.Instance.CurrentRegion.RegionEquals(regionInfo))
            {
                __instance.defaultButtonSelected = __instance.firstOption;
                __instance.firstOption.ChangeButtonText(
                    TranslationController.Instance.GetStringWithDefault(
                        regionInfo.TranslateName,
                        regionInfo.Name));
            }
            else
            {
                var region = regionInfo;
                var serverListButton = __instance.ButtonPool.Get<ServerListButton>();
                var x = num % 2 == 0 ? -2 : 2;
                if (findingGame)
                {
                    x += 2;
                }
                var y = -0.55f * (num / 2f);
                serverListButton.transform.localPosition = new Vector3(x, __instance.y_posButton + y, -1f);
                serverListButton.transform.localScale = Vector3.one;
                serverListButton.Text.text =
                    TranslationController.Instance.GetStringWithDefault(
                        regionInfo.TranslateName,
                        regionInfo.Name);
                serverListButton.Text.ForceMeshUpdate();
                serverListButton.Button.OnClick.RemoveAllListeners();
                serverListButton.Button.OnClick.AddListener((UnityAction)(() => { __instance.ChooseOption(region); }));
                __instance.controllerSelectable.Add(serverListButton.Button);
                __instance.background.transform.localPosition = new Vector3(
                    findingGame ? 2f : 0f,
                    __instance.initialYPos + -0.3f * (num / 2f),
                    0f);
                __instance.background.size = new Vector2(__instance.background.size.x, 1.2f + 0.6f * (num / 2f));
                num++;
            }
        }

        return false;
    }
}
