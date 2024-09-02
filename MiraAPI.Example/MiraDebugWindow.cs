using Reactor.Utilities.Attributes;
using Reactor.Utilities.ImGui;
using System;
using UnityEngine;

namespace MiraAPI.Example;

[RegisterInIl2Cpp]
public class MiraDebugWindow(IntPtr cppPtr) : MonoBehaviour(cppPtr)
{
    public DragWindow DebuggingWindow { get; } = new(
        new Rect(10, 10, 0, 0),
        "MIRA API DEBUGGING",
        () =>
        {
        })
    {
        Enabled = true,
    };

    public void OnGUI()
    {
        DebuggingWindow.OnGUI();
    }
}
