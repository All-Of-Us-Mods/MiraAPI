using Reactor.Utilities.Attributes;
using Reactor.Utilities.ImGui;
using System;
using UnityEngine;

namespace MiraAPI.Example;
[RegisterInIl2Cpp]
public class MiraDebugWindow(IntPtr ptr) : MonoBehaviour(ptr)
{
    public readonly DragWindow DebuggingWindow = new(new Rect(10, 10, 0, 0), "MIRA API DEBUGGING", () =>
    {
        if (GUILayout.Button("Test modifier"))
        {
            //PlayerControl.LocalPlayer.AddModifier<ModifierTimerExample>();
        }
        if (GUILayout.Button("Remove modifier"))
        {
            //PlayerControl.LocalPlayer.RemoveModifier<ModifierTimerExample>();
        }
    })
    {
        Enabled = true,
    };

    public void OnGUI()
    {
        DebuggingWindow.OnGUI();
    }
}
