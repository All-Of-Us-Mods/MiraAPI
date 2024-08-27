#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches.Options;

[HarmonyPatch(typeof(LobbyViewSettingsPane))]
public static class LobbyViewPanePatches
{
    
    public static int CurrentSelectedMod { get; private set; }

    public static MiraPluginInfo? SelectedMod => CurrentSelectedMod == 0 ? null : MiraPluginManager.Instance.RegisteredPlugins.ElementAt(CurrentSelectedMod-1).Value;


    [HarmonyPostfix]
    [HarmonyPatch(nameof(LobbyViewSettingsPane.Awake))]
    public static void AwakePatch(LobbyViewSettingsPane __instance)
    {
        __instance.gameModeText.transform.localPosition = new Vector3(-2.3f, 2.4f, -2);

        // Create the next button
        var nextButton = Object.Instantiate(__instance.BackButton, __instance.BackButton.transform.parent).gameObject;
        nextButton.GetComponent<BoxCollider2D>().size = new Vector2(0.2f, 0.3f);
        nextButton.transform.localPosition = new Vector3(-5.4f, 2.4f, -2f);
        nextButton.transform.localScale = new Vector3(3, 3, 2);
        nextButton.name = "RightArrowButton";
        
        var normal = nextButton.transform.FindChild("Normal").GetComponentInChildren<SpriteRenderer>();
        normal.transform.localPosition = new Vector3(0, 0f, 0.3f);
        normal.sprite = MiraAssets.NextButton.LoadAsset();
        
        var hover = nextButton.transform.FindChild("Hover").GetComponentInChildren<SpriteRenderer>();
        hover.transform.localPosition = new Vector3(0, 0f, 0.3f);
        hover.sprite = MiraAssets.NextButtonActive.LoadAsset();
        
        var passiveButton = nextButton.gameObject.GetComponent<PassiveButton>();
        passiveButton.OnClick = new Button.ButtonClickedEvent();
        passiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            CurrentSelectedMod += 1;
            if (CurrentSelectedMod > MiraPluginManager.Instance.RegisteredPlugins.Count)
            {
                CurrentSelectedMod = 0;
            }
            __instance.RefreshTab();
            __instance.scrollBar.ScrollToTop();
        }));

        // Create the back button
        var backButton = Object.Instantiate(nextButton, __instance.BackButton.transform.parent).gameObject;
        backButton.transform.localPosition = new Vector3(-6.3f, 2.4f, -2f);
        backButton.name = "LeftArrowButton";
        backButton.transform.FindChild("Normal").gameObject.GetComponentInChildren<SpriteRenderer>().flipX
            = backButton.transform.FindChild("Hover").gameObject.GetComponentInChildren<SpriteRenderer>().flipX 
                = true;
        
        var passiveButton2 = backButton.gameObject.GetComponent<PassiveButton>();
        passiveButton2.OnClick = new Button.ButtonClickedEvent();
        passiveButton2.OnClick.AddListener((UnityAction)(() =>
        {
            CurrentSelectedMod -= 1;
            if (CurrentSelectedMod < 0)
            {
                CurrentSelectedMod = MiraPluginManager.Instance.RegisteredPlugins.Count;
            }
            __instance.RefreshTab();
            __instance.scrollBar.ScrollToTop();
        }));
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(nameof(LobbyViewSettingsPane.Update))]
    public static void UpdatePatch(LobbyViewSettingsPane __instance)
    {
        __instance.gameModeText.text = SelectedMod?.PluginInfo.Metadata.Name ?? "Default";
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(nameof(LobbyViewSettingsPane.DrawNormalTab))]
    public static bool DrawNormalTabPatch(LobbyViewSettingsPane __instance)
    {
	    if (CurrentSelectedMod == 0)
	    {
		    return true;
	    }
        
	    DrawOptionsTab(__instance);
	    return false;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(nameof(LobbyViewSettingsPane.DrawRolesTab))]
    public static bool DrawRolesTabPatch(LobbyViewSettingsPane __instance)
    {
	    if (CurrentSelectedMod == 0)
	    {
		    return true;
	    }
        
	    DrawRolesTab(__instance);
	    return false;
    }
    
    private static void DrawOptionsTab(LobbyViewSettingsPane instance)
    {
        if (SelectedMod == null)
        {
            return;
        }
        
        var num = 1.44f;
        
        var filteredGroups = SelectedMod.OptionGroups.Where(x => x.GroupVisible.Invoke() && x.AdvancedRole is null);

        foreach (var group in filteredGroups)
        {
            if (group.Options.Count == 0)
            {
                continue;
            }
            
            var categoryHeaderMasked = Object.Instantiate(instance.categoryHeaderOrigin, instance.settingsContainer, true);
            categoryHeaderMasked.SetHeader(StringNames.Name, 61);
            categoryHeaderMasked.Title.text = group.GroupName;
            categoryHeaderMasked.transform.localScale = Vector3.one;
            categoryHeaderMasked.transform.localPosition = new Vector3(-9.77f, num, -2f);
            instance.settingsInfo.Add(categoryHeaderMasked.gameObject);
            num -= 0.85f;

            var i = 0;

            foreach (var option in group.Options)
            {
                if (!option.Visible.Invoke())
                {
                    continue;
                }
                
                var viewSettingsInfoPanel = Object.Instantiate(instance.infoPanelOrigin, instance.settingsContainer, true);
                viewSettingsInfoPanel.transform.localScale = Vector3.one;
                float num2;
                if (i % 2 == 0)
                {
                    num2 = -8.95f;
                    if (i > 0)
                    {
                        num -= 0.59f;
                    }
                }
                else
                {
                    num2 = -3f;
                }
                viewSettingsInfoPanel.transform.localPosition = new Vector3(num2, num, -2f);

                var data = option.Data;
                
                if (data.Type == OptionTypes.Checkbox)
                {
                    viewSettingsInfoPanel.SetInfoCheckbox(data.Title, 61, Mathf.Approximately(option.GetFloatData(), 1));
                }
                else
                {
                    viewSettingsInfoPanel.SetInfo(data.Title, data.GetValueString(option.GetFloatData()), 61);
                }
                
                instance.settingsInfo.Add(viewSettingsInfoPanel.gameObject);
                i++;
            }
            
            num -= 0.59f;
        }
        instance.scrollBar.CalculateAndSetYBounds(instance.settingsInfo.Count + 10, 2f, 6f, 0.59f);
    }

    private static void DrawRolesTab(LobbyViewSettingsPane instance)
    {
	    if (SelectedMod == null)
	    {
		    return;
	    }
	    
	    var num = 0.95f;
	    var num2 = -6.53f;
	    var categoryHeaderMasked =
		    Object.Instantiate(instance.categoryHeaderOrigin, instance.settingsContainer, true);
	    categoryHeaderMasked.SetHeader(StringNames.RoleQuotaLabel, 61);
	    categoryHeaderMasked.transform.localScale = Vector3.one;
	    categoryHeaderMasked.transform.localPosition = new Vector3(-9.77f, 1.26f, -2f);
	    instance.settingsInfo.Add(categoryHeaderMasked.gameObject);
	    
	    var list = new List<RoleRulesCategory>();

	    foreach (var team in Enum.GetValues<ModdedRoleTeams>())
	    {
		    var filteredRoles = SelectedMod.CustomRoles.Values
			    .OfType<ICustomRole>()
			    .Where(x => !x.HideSettings && x.Team == team).ToList();
		    
		    if (filteredRoles.Count == 0)
		    {
			    continue;
		    }
		    
		    var categoryHeaderRoleVariant =
			    Object.Instantiate(instance.categoryHeaderRoleOrigin, instance.settingsContainer, true);

		    switch (team)
		    {
			    case ModdedRoleTeams.Crewmate:
				    categoryHeaderRoleVariant.SetHeader(StringNames.CrewmateRolesHeader, 61);
				    break;
			    case ModdedRoleTeams.Impostor:
				    categoryHeaderRoleVariant.SetHeader(StringNames.ImpostorRolesHeader, 61);
				    break;
			    case ModdedRoleTeams.Neutral:
			    default:
				    categoryHeaderRoleVariant.SetHeader(StringNames.CrewmateRolesHeader, 61);
				    categoryHeaderRoleVariant.Title.text = team + " Roles";
				    categoryHeaderRoleVariant.Background.color = Color.gray;
				    categoryHeaderRoleVariant.Title.color = Color.white;
				    break;
		    }
		    
		    categoryHeaderRoleVariant.transform.localScale = Vector3.one;
		    categoryHeaderRoleVariant.transform.localPosition = new Vector3(0.09f, num, -2f);
		    instance.settingsInfo.Add(categoryHeaderRoleVariant.gameObject);
		    num -= 0.696f;

		    foreach (var role in filteredRoles)
		    {
			    var roleBehaviour = role as RoleBehaviour;
			    if (roleBehaviour == null)
			    {
				    continue;
			    }
			    
			    var chancePerGame =
				    GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(roleBehaviour.Role);
			    var numPerGame =
				    GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(roleBehaviour.Role);
			    
			    var flag = numPerGame == 0;
			    
			    var viewSettingsInfoPanelRoleVariant =
				    Object.Instantiate(
					    instance.infoPanelRoleOrigin, instance.settingsContainer, true);
			    viewSettingsInfoPanelRoleVariant.transform.localScale = Vector3.one;
			    viewSettingsInfoPanelRoleVariant.transform.localPosition = new Vector3(num2, num, -2f);
			    
			    if (!flag)
			    {
				    //list.Add(role.GetType());
			    }

			    viewSettingsInfoPanelRoleVariant.SetInfo(roleBehaviour.NiceName, numPerGame, chancePerGame,
				    61, role.RoleColor, role.Icon.LoadAsset(), true);
			    
			    viewSettingsInfoPanelRoleVariant.titleText.color = 
				    viewSettingsInfoPanelRoleVariant.chanceTitle.color = 
					    viewSettingsInfoPanelRoleVariant.chanceBackground.color = 
						    viewSettingsInfoPanelRoleVariant.background.color = 
							    role.RoleColor.GetAlternateColor();
			    instance.settingsInfo.Add(viewSettingsInfoPanelRoleVariant.gameObject);
			    num -= 0.664f;
		    }


	    }

	    if (list.Count > 0)
	    {
		    var categoryHeaderMasked2 =
			    Object.Instantiate(instance.categoryHeaderOrigin, instance.settingsContainer, true);
		    categoryHeaderMasked2.SetHeader(StringNames.RoleSettingsLabel, 61);
		    categoryHeaderMasked2.transform.localScale = Vector3.one;
		    categoryHeaderMasked2.transform.localPosition = new Vector3(-9.77f, num, -2f);
		    instance.settingsInfo.Add(categoryHeaderMasked2.gameObject);
		    num -= 1.7f;
		    var num3 = 0f;
		    for (var k = 0; k < list.Count; k++)
		    {
			    float num4;
			    if (k % 2 == 0)
			    {
				    num4 = -5.8f;
				    if (k > 0)
				    {
					    num -= num3 + 0.59f;
					    num3 = 0f;
				    }
			    }
			    else
			    {
				    num4 = 0.14999962f;
			    }

			    var advancedRoleViewPanel =
				    Object.Instantiate(instance.advancedRolePanelOrigin, instance.settingsContainer, true);
			    advancedRoleViewPanel.transform.localScale = Vector3.one;
			    advancedRoleViewPanel.transform.localPosition = new Vector3(num4, num, -2f);
			    var num5 = advancedRoleViewPanel.SetUp(list[k], 0.59f, 61);
			    if (num5 > num3)
			    {
				    num3 = num5;
			    }

			    instance.settingsInfo.Add(advancedRoleViewPanel.gameObject);
		    }
	    }

	    instance.scrollBar.SetYBoundsMax(-num);
    }
}