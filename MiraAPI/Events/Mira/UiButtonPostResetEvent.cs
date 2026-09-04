using UnityEngine;

namespace MiraAPI.Events.Mira;

/// <summary>
/// Event fired by Mira API to adjust the UI buttons at the top right of the screen.
/// </summary>
public class UiButtonPostResetEvent : MiraEvent
{
    /// <summary>
    /// Gets the <see cref="GameObject"/> parent for the very top UI row.
    /// </summary>
    public GameObject MainTopUiRow { get; }

    /// <summary>
    /// Gets the <see cref="GameObject"/> parent for the second top UI row.
    /// </summary>
    public GameObject SecondTopUiRow { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UiButtonPostResetEvent"/> class.
    /// </summary>
    /// <param name="topUiRow">The <see cref="GameObject"/> for the top UI row.</param>
    /// <param name="secondUiRow">The <see cref="GameObject"/> for the second UI row.</param>
    public UiButtonPostResetEvent(GameObject topUiRow, GameObject secondUiRow)
    {
        MainTopUiRow = topUiRow;
        SecondTopUiRow = secondUiRow;
    }
}
