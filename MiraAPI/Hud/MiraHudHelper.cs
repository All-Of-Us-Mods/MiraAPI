using BepInEx.Unity.IL2CPP.Utils.Collections;
using InnerNet;
using MiraAPI.LocalSettings;
using MiraAPI.Modifiers.ModifierDisplay;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace MiraAPI.Hud;

[RegisterInIl2Cpp]
public sealed class MiraHudHelper(nint cppPtr) : MonoBehaviour(cppPtr)
{
    public static MiraHudHelper Instance { get; private set; }
    public static GameObject ModifierDisplayObject;
    public static GameObject VanillaMatchInfoButton;
    public static bool ModifierDisplayOnRight;
    public static GameObject ClonedChatButton;
    public static GameObject ExtraUiTopRight;
    public static GridArrange ExtraUiGrid;
    public static AspectPosition ExtraUiAspectPos;
    public static GameObject UiTopRight;
    public static GridArrange UiGrid;
    public static AspectPosition UiAspectPos;
    public static GameObject SubmergedFloorButton;
    public static SpriteRenderer SubmergedFloorButtonRenderer;
    public static SpriteRenderer SubmergedFloorButtonRendererHover;
    public void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        MiraApiSettings.OldButtonScaleFactor =
            LocalSettingsTabSingleton<MiraApiSettings>.Instance.TopRightButtonsFactorSlider.Value;
        StartCoroutine(MiraApiSettings.CoResizeSettingsUI().WrapToIl2Cpp());
    }
#pragma warning disable S2325
    #pragma warning disable CA1822
    public void FixedUpdate()
    {
        if (!HudManager.InstanceExists || !PlayerControl.LocalPlayer || !PlayerControl.LocalPlayer.Data)
        {
            return;
        }

        var instance = HudManager.Instance;

        CreateUiRow(instance);
        CreateNewUiRow(instance);
        AdjustModifierTab();

        if (!PlayerControl.LocalPlayer || !PlayerControl.LocalPlayer.Data || !PlayerControl.LocalPlayer.Data.Role ||
            !ShipStatus.Instance ||
            (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started &&
             !TutorialManager.InstanceExists))
        {
            return;
        }
        UpdateSubmergedButtons(instance);
    }
    #pragma warning restore CA1822
    #pragma warning restore S2325

    public static Vector3 BelowOptionPos = new(0.435f, 1.25f, 0f);
    public static Vector3 FullTopPos = new(0.435f, 0.475f, 0f);
    public static void CreateUiRow(HudManager instance)
    {
        if (!UiTopRight)
        {
            UiTopRight = instance.MapButton.transform.parent.gameObject;

            UiGrid = UiTopRight.AddComponent<GridArrange>();
            UiAspectPos = UiTopRight.AddComponent<AspectPosition>();

            UiGrid.Alignment = GridArrange.StartAlign.Left;
            UiGrid.CellSize = new Vector2(0.85f, 0.85f);
            UiGrid.MaxColumns = 6;
            UiAspectPos.Alignment = AspectPosition.EdgeAlignments.RightTop;
            UiGrid.Start();
            UiAspectPos.DistanceFromEdge = FullTopPos;
            var mapButton = instance.MapButton.gameObject;
            mapButton.GetComponent<AspectPosition>().Destroy();
            var settingsButton = instance.SettingsButton;
            settingsButton.GetComponent<AspectPosition>().Destroy();
            VanillaMatchInfoButton = instance.MatchInfoButton.gameObject;
            VanillaMatchInfoButton.GetComponent<AspectPosition>().Destroy();
            var oldPos = settingsButton.transform.localPosition;
            settingsButton.transform.localPosition = new Vector3(oldPos.x, oldPos.y, -100);
            var chatButton = instance.Chat.chatButton.gameObject;
            ClonedChatButton = Object.Instantiate(chatButton, chatButton.transform.parent);
            ClonedChatButton.SetActive(false);
            instance.Chat.chatButtonAspectPosition = ClonedChatButton.GetComponent<AspectPosition>();
            chatButton.GetComponent<AspectPosition>().Destroy();
            var inactivePos = settingsButton.transform.GetChild(1).transform.localPosition;
            var bg = settingsButton.transform.GetChild(2).gameObject;
            var bgPos = bg.transform.localPosition;
            var bgSprite = bg.GetComponent<SpriteRenderer>().sprite;
            var activePos = settingsButton.transform.GetChild(3).transform.localPosition;
            var selectedPos = settingsButton.transform.GetChild(4).transform.localPosition;
            chatButton.transform.GetChild(2).transform.localPosition = inactivePos;
            var chatBg = chatButton.transform.GetChild(3);
            chatBg.transform.localPosition = bgPos;
            chatBg.GetComponent<SpriteRenderer>().sprite = bgSprite;
            chatButton.transform.GetChild(4).transform.localPosition = activePos;
            chatButton.transform.GetChild(5).transform.localPosition = selectedPos;
            var collider = chatButton.GetComponent<BoxCollider2D>();
            collider.size = new Vector2(0.4354f, 0.4003f);
            collider.offset = new Vector2(0.0025f, 0.0254f);
            if (FriendsListManager.InstanceExists && !TutorialManager.InstanceExists)
            {
                var listButton = FriendsListManager.Instance.FriendsListButton.transform.GetChild(0);
                listButton.transform.SetParent(UiTopRight.transform, false);
                FriendsListManager.Instance.FriendsListButton = listButton.GetComponent<FriendsListButton>();
                listButton.GetComponent<AspectPosition>().Destroy();
                listButton.localPosition = new Vector3(0, 0, 0);
            }
            settingsButton.transform.SetAsLastSibling();
            chatButton.transform.SetParent(UiTopRight.transform, false);
            instance.Chat.chatButton = chatButton.GetComponent<PassiveButton>();
            var iconContainer = new GameObject("iconContainer")
            {
                layer = LayerMask.NameToLayer("UI"),
            };
            iconContainer.transform.SetParent(chatButton.transform, false);
            iconContainer.transform.localPosition = new Vector3(0.1f, -0.1f, 0);
            instance.Chat.chatNotifyDot.transform.SetParent(iconContainer.transform, false);
            instance.Chat.chatNotifyDot = iconContainer.transform.GetChild(0).GetComponent<SpriteRenderer>();
        }

        if (UiTopRight && UiGrid)
        {
            var isChatButtonVisible = HudManager.Instance.Chat.isActiveAndEnabled;
            instance.Chat.chatButton.gameObject.SetActive(isChatButtonVisible);
            if (VanillaMatchInfoButton)
            {
                VanillaMatchInfoButton.SetActive(!GameSettingMenu.Instance && !Minigame.Instance);
            }
        }
    }
    public static void UpdateSubmergedButtons(HudManager instance)
    {
        if (ModCompatibility.IsSubmerged() && !SubmergedFloorButton && ExtraUiTopRight)
        {
            if (!SubmergedFloorButton && ExtraUiTopRight)
            {
                var transform = instance.MapButton.transform.parent.Find(instance.MapButton.name + "(Clone)");
                if (transform != null)
                {
                    SubmergedFloorButton = transform.gameObject;
                    SubmergedFloorButton.transform.SetParent(ExtraUiTopRight.transform, false);

                    SubmergedFloorButtonRenderer =
                        SubmergedFloorButton.transform.Find("Inactive").GetComponent<SpriteRenderer>();
                    SubmergedFloorButtonRendererHover =
                        SubmergedFloorButton.transform.Find("Active").GetComponent<SpriteRenderer>();

                    MiraApiSettings.SetUpButtonPositions();
                }
            }
        }
    }

    public static void AdjustModifierTab()
    {
        if (!ModifierDisplayObject && UiTopRight && ExtraUiTopRight && ModifierDisplayComponent.Instance)
        {
            ModifierDisplayObject = ModifierDisplayComponent.Instance?.gameObject ?? null!;
            ModifierDisplayOnRight = !LocalSettingsTabSingleton<MiraApiSettings>.Instance.ModifiersHudLeftSide.Value;
            if (ModifierDisplayOnRight)
            {
                ModifierDisplayObject.transform.SetParent(ExtraUiTopRight.transform, false);
                ModifierDisplayObject.GetComponent<AspectPosition>().Destroy();
                var oldPos = ModifierDisplayObject.transform.GetChild(0).localPosition;
                ModifierDisplayObject.transform.GetChild(0).localPosition = new Vector3(-1.1757f, -2.1633f, oldPos.z);
                oldPos = ModifierDisplayObject.transform.GetChild(1).localPosition;
                ModifierDisplayObject.transform.GetChild(1).localPosition = new Vector3(-0.45f, 0.3f, oldPos.z);
            }
            MiraApiSettings.SetUpButtonPositions();
        }
    }
    public static void CreateNewUiRow(HudManager instance)
    {
        if (!ExtraUiTopRight && UiTopRight)
        {
            ExtraUiTopRight = new GameObject("ExtraUiTopRight")
            {
                layer = UiTopRight.layer,
            };
            ExtraUiTopRight.transform.SetParent(instance.MapButton.transform.parent.parent, false);

            ExtraUiGrid = ExtraUiTopRight.AddComponent<GridArrange>();
            ExtraUiAspectPos = ExtraUiTopRight.AddComponent<AspectPosition>();

            ExtraUiGrid.Alignment = GridArrange.StartAlign.Left;
            ExtraUiGrid.CellSize = new Vector2(0.85f, 0.85f);
            ExtraUiAspectPos.Alignment = AspectPosition.EdgeAlignments.RightTop;
            ExtraUiAspectPos.DistanceFromEdge = BelowOptionPos;
            ExtraUiGrid.Start();
        }
    }
}
