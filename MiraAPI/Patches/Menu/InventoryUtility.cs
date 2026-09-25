using System;
using MiraAPI.Utilities.Assets;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches.Menu;

public static class InventoryUtility
{
    public static void CreateNextBackButtons<T>(T tab, Action<T> previousPage, Action<T> nextPage)
        where T : InventoryTab
    {
        var title = tab.transform.FindChild("Text");
        title.localPosition = new Vector3(.9123f, -.23f, -55);
        title.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.Center;

        tab.scroller.Hitbox.transform.localScale -= new Vector3(0, 0.1f, 0);
        tab.scroller.Hitbox.transform.localPosition -= new Vector3(0, 0.1f, 0);

        var leftButtonElement = Object.Instantiate(PlayerCustomizationMenu.Instance.BackButton.gameObject, tab.transform);
        var leftSpriteRenderer = leftButtonElement.GetComponent<SpriteRenderer>();
        var leftButton = leftButtonElement.GetComponent<PassiveButton>();

        leftButtonElement.transform.localPosition = new Vector3(-1.1f, -0.2f, -55);
        leftSpriteRenderer.sprite = MiraAssets.NextButton.LoadAsset();
        leftSpriteRenderer.flipX = true;

        leftButton.OnMouseOver = new UnityEvent();
        leftButton.OnMouseOver.AddListener((Action)(() => leftSpriteRenderer.sprite = MiraAssets.NextButtonActive.LoadAsset()));
        leftButton.OnMouseOut = new UnityEvent();
        leftButton.OnMouseOut.AddListener((Action)(() => leftSpriteRenderer.sprite = MiraAssets.NextButton.LoadAsset()));

        leftButton.OnClick = new Button.ButtonClickedEvent();
        leftButton.OnClick.AddListener((Action)(() => previousPage(tab)));

        var rightButtonElement = Object.Instantiate(leftButtonElement, tab.transform);
        var rightSpriteRenderer = rightButtonElement.GetComponent<SpriteRenderer>();
        var rightButton = rightButtonElement.GetComponent<PassiveButton>();

        rightButtonElement.transform.localPosition = new Vector3(2.8f, -0.2f, -55);
        rightSpriteRenderer.sprite = MiraAssets.NextButton.LoadAsset();
        rightSpriteRenderer.flipX = false;

        rightButton.OnMouseOver = new UnityEvent();
        rightButton.OnMouseOver.AddListener((Action)(() => rightSpriteRenderer.sprite = MiraAssets.NextButtonActive.LoadAsset()));
        rightButton.OnMouseOut = new UnityEvent();
        rightButton.OnMouseOut.AddListener((Action)(() => rightSpriteRenderer.sprite = MiraAssets.NextButton.LoadAsset()));

        rightButton.OnClick = new Button.ButtonClickedEvent();
        rightButton.OnClick.AddListener((Action)(() => nextPage(tab)));
    }
}
