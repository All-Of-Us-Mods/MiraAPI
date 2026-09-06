using System;
using System.Diagnostics.CodeAnalysis;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RegisterInIl2Cpp]
// TODO: Give proper reasoning
[SuppressMessage("Design", "CA1050:Declare types in namespaces", Justification = "Reason pending.")]
[SuppressMessage("Major Bug", "S3903:Types should be defined in named namespaces", Justification = "Reason pending.")]
[SuppressMessage("Style", "IDE0051:Remove unused private members", Justification = "Unity Convention.")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member (Justification: Unity fields; ignore.)
public class SavePresetPopup(nint cppPtr) : Minigame(cppPtr)
{
    private TextMeshPro textBoxText;
    private TextBoxTMP textBox;

    private Action<string> onSave;

    // Cleanup holder object
    private void OnDestroy()
    {
        transform.parent.gameObject.Destroy();
    }

    public override void Close()
    {
        // no-op
    }

    private void Awake()
    {
        var textboxHolder = transform.GetChild(1).GetChild(1);
        var saveButton = transform.FindChild("SaveButton").GetComponent<PassiveButton>();
        var closeButton = transform.FindChild("CloseButton").GetComponent<PassiveButton>();
        textBox = textboxHolder.GetChild(0).GetComponent<TextBoxTMP>();
        textBoxText = textBox.transform.GetChild(0).GetComponent<TextMeshPro>();

        saveButton.OnClick = new Button.ButtonClickedEvent();
        saveButton.OnClick.AddListener((UnityAction)(() =>
        {
            onSave.Invoke(textBoxText.text);
            this.BaseClose();
        }));

        closeButton.OnClick = new Button.ButtonClickedEvent();
        closeButton.OnClick.AddListener((UnityAction)(() =>
        {
            this.BaseClose();
        }));

        textBox.OnChange = new Button.ButtonClickedEvent();
        textBox.OnChange.AddListener((UnityAction)(() =>
        {
            textBoxText.text = textBoxText.text.Replace(" ", string.Empty);
        }));

        textBox.GetComponent<PassiveButton>().OnClick.AddListener((UnityAction)(() =>
        {
            textBox.GiveFocus();
        }));

        foreach (var tmp in gameObject.GetComponentsInChildren<TextMeshPro>())
        {
            if (tmp.gameObject.transform.parent.name == "Textbox")
            {
                continue;
            }

            var text = tmp.text;
            tmp.text = $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF RadialMenu Material\">{text}</font>";
        }
    }

    public static void CreatePopup(Action<string> saveAction)
    {
        // Because innerscuff doesn't account for Z value in Close animation
        var holder = new GameObject("PopupHolder")
        {
            transform =
            {
                parent = Camera.main!.transform,
                localPosition = new Vector3(0, -1f, -500),
            },
        };

        var gameObject = Instantiate(MiraAssets.PresetSavePopup.LoadAsset(), holder.transform);
        var popup = gameObject.GetComponent<SavePresetPopup>();
        popup.onSave = saveAction;
        popup.Begin(null);
    }
}
