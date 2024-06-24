using HarmonyLib;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.UI.Button;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches.Options
{
    [HarmonyPatch(typeof(RolesSettingsMenu))]
    public static class RoleSettingMenuPatches
    {
        [HarmonyPrefix, HarmonyPatch(nameof(RolesSettingsMenu.SetQuotaTab))]
        public static bool PatchStart(RolesSettingsMenu __instance)
        {
            if (GameSettingMenuPatches.currentSelectedMod == 0)
            {
                __instance.scrollBar.transform.localPosition = new Vector3(-1.4957f, 0.657f, -4);
                return true;
            }

            float num = 0.662f;

            __instance.AllButton.transform.parent.gameObject.SetActive(false);
            __instance.AllButton.gameObject.SetActive(false);
            __instance.scrollBar.transform.localPosition = new Vector3(-1.4957f, 1.5261f, -4);

            CategoryHeaderEditRole categoryHeaderEditRole = Object.Instantiate(__instance.categoryHeaderEditRoleOrigin, Vector3.zero, Quaternion.identity, __instance.RoleChancesSettings.transform);
            categoryHeaderEditRole.SetHeader(StringNames.CrewmateRolesHeader, 20);
            categoryHeaderEditRole.transform.localPosition = new Vector3(4.986f, num, -2f);
            num -= 0.522f;
            int num3 = 0;

            foreach (var role in GameSettingMenuPatches.selectedMod.CustomRoles.Where(role => role.Value.TeamType == RoleTeamTypes.Crewmate))
            {
                CreateQuotaOption(__instance, role.Value, ref num, num3);
                num3++;
            }

            num -= 0.4f;
            CategoryHeaderEditRole categoryHeaderEditRole2 = Object.Instantiate(__instance.categoryHeaderEditRoleOrigin, Vector3.zero, Quaternion.identity, __instance.RoleChancesSettings.transform);
            categoryHeaderEditRole2.SetHeader(StringNames.ImpostorRolesHeader, 20);
            categoryHeaderEditRole2.transform.localPosition = new Vector3(4.986f, num, -2f);
            num -= 0.522f;

            foreach (var role in GameSettingMenuPatches.selectedMod.CustomRoles.Where(role => role.Value.TeamType == RoleTeamTypes.Impostor))
            {
                CreateQuotaOption(__instance, role.Value, ref num, num3);
                num3++;
            }

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(nameof(RolesSettingsMenu.Update))]
        public static bool UpdatePatch()
        {
            if (GameSettingMenuPatches.currentSelectedMod == 0)
            {
                return true;
            }

            return false;
        }

        private static void ValueChanged(OptionBehaviour obj)
        {
            var roleSetting = obj.Cast<RoleOptionSetting>();
            var role = roleSetting.Role as ICustomRole;
            if (role.HideSettings) return;

            MiraAPIPlugin.Instance.Config.TryGetEntry<int>(role.NumConfigDefinition, out var numEntry);
            numEntry.Value = roleSetting.RoleMaxCount;

            MiraAPIPlugin.Instance.Config.TryGetEntry<int>(role.ChanceConfigDefinition, out var chanceEntry);
            chanceEntry.Value = roleSetting.RoleChance;

            roleSetting.UpdateValuesAndText(GameOptionsManager.Instance.CurrentGameOptions.RoleOptions);

            /*            if (AmongUsClient.Instance.AmHost)
                        {
                            Rpc<SyncRoleOptionsRpc>.Instance.Send(new SyncRoleOptionsRpc.Data((ushort)roleSetting.Role.Role, numEntry.Value, chanceEntry.Value));
                        }*/
            GameOptionsManager.Instance.GameHostOptions = GameOptionsManager.Instance.CurrentGameOptions;
            return;
        }

        private static void CreateAdvancedSettings(RolesSettingsMenu __instance, RoleBehaviour role)
        {
            foreach (var option in __instance.advancedSettingChildren) 
            {
                Object.Destroy(option.gameObject);
            }

            __instance.advancedSettingChildren.Clear();

            float num = -0.872f;
            foreach (var option in GameSettingMenuPatches.selectedMod.Options.Where(option => option.AdvancedRole == role.GetType()))
            {
                OptionBehaviour newOpt = option.CreateOption(__instance.checkboxOrigin, __instance.numberOptionOrigin, __instance.stringOptionOrigin, __instance.AdvancedRolesSettings.transform);
                newOpt.transform.localPosition = new Vector3(2.17f, num, -2f);
                newOpt.SetClickMask(__instance.ButtonClickMask);

                SpriteRenderer[] componentsInChildren = newOpt.GetComponentsInChildren<SpriteRenderer>(true);
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    componentsInChildren[i].material.SetInt(PlayerMaterial.MaskLayer, 20);
                }
                foreach (TextMeshPro textMeshPro in newOpt.GetComponentsInChildren<TextMeshPro>(true))
                {
                    textMeshPro.fontMaterial.SetFloat("_StencilComp", 3f);
                    textMeshPro.fontMaterial.SetFloat("_Stencil", 20);
                }

                newOpt.LabelBackground.enabled = false;
                __instance.advancedSettingChildren.Add(newOpt);

                num += -0.45f;
                newOpt.Initialize();
            }

            __instance.scrollBar.CalculateAndSetYBounds((float)(__instance.advancedSettingChildren.Count + 3), 1f, 6f, 0.45f);
        }
        private static void ChangeTab(RoleBehaviour role, RolesSettingsMenu __instance)
        {
            ICustomRole customRole = role as ICustomRole;
            __instance.roleDescriptionText.text = customRole.RoleLongDescription;
            __instance.roleTitleText.text = DestroyableSingleton<TranslationController>.Instance.GetString(role.StringName, Array.Empty<Il2CppSystem.Object>());
            __instance.roleScreenshot.sprite = Sprite.Create(role.RoleScreenshot.texture, new Rect(0, 0, 370, 230), Vector2.one / 2, 100);
            __instance.roleScreenshot.drawMode = SpriteDrawMode.Sliced;
            __instance.roleHeaderSprite.color = customRole.RoleColor;
            __instance.roleHeaderText.color = new Color(customRole.RoleColor.r - 0.3f, customRole.RoleColor.g - 0.3f, customRole.RoleColor.b - 0.3f);

            CreateAdvancedSettings(__instance, role);

            foreach (var optionBehaviour in __instance.advancedSettingChildren)
            {
                optionBehaviour.OnValueChanged = new Action<OptionBehaviour>(__instance.ValueChanged);
                if (AmongUsClient.Instance && !AmongUsClient.Instance.AmHost)
                {
                    optionBehaviour.SetAsPlayer();
                }
            }
            __instance.RoleChancesSettings.SetActive(false);
            __instance.AdvancedRolesSettings.SetActive(true);
            __instance.RefreshChildren();
        }

        public static void CreateQuotaOption(RolesSettingsMenu __instance, RoleBehaviour role, ref float yPos, int index)
        {
            RoleOptionSetting roleOptionSetting = Object.Instantiate(__instance.roleOptionSettingOrigin, Vector3.zero, Quaternion.identity, __instance.RoleChancesSettings.transform);
            roleOptionSetting.transform.localPosition = new Vector3(-0.15f, yPos, -2f);
            roleOptionSetting.SetRole(GameOptionsManager.Instance.CurrentGameOptions.RoleOptions, role, 20);
            roleOptionSetting.labelSprite.color = (role as ICustomRole).RoleColor;
            roleOptionSetting.OnValueChanged = new Action<OptionBehaviour>(ValueChanged);
            roleOptionSetting.titleText.horizontalAlignment = TMPro.HorizontalAlignmentOptions.Left;
            roleOptionSetting.SetClickMask(__instance.ButtonClickMask);
            __instance.roleChances.Add(roleOptionSetting);

            PassiveButton newButton = Object.Instantiate(roleOptionSetting.buttons[0], roleOptionSetting.transform);
            newButton.name = "ConfigButton";
            newButton.transform.localPosition = new Vector3(0.2419f, -0.2582f, -2f);
            newButton.transform.FindChild("Plus_TMP").gameObject.DestroyImmediate();
            newButton.transform.FindChild("InactiveSprite").GetComponent<SpriteRenderer>().sprite = MiraAssets.Cog.LoadAsset();
            newButton.transform.FindChild("ActiveSprite").GetComponent<SpriteRenderer>().sprite = MiraAssets.CogActive.LoadAsset();

            PassiveButton passiveButton = newButton.GetComponent<PassiveButton>();
            passiveButton.OnClick = new ButtonClickedEvent();
            passiveButton.OnClick.AddListener((UnityAction)(() =>
            {
                ChangeTab(role, __instance);
            }));

            if (index < GameSettingMenuPatches.selectedMod.CustomRoles.Count - 1)
            {
                yPos += -0.43f;
            }
        }
    }
}
