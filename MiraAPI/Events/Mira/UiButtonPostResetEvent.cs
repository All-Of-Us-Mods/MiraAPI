using UnityEngine;

namespace MiraAPI.Events.Mira;

/// <summary>
/// Event fired by Mira API to adjust the UI buttons at the top right of the screen.
/// </summary>
/// <param name="topUiRow">The <see cref="GameObject"/> for the top UI row.</param>
/// <param name="secondUiRow">The <see cref="GameObject"/> for the second UI row.</param>
public class UiButtonPostResetEvent(GameObject topUiRow, GameObject secondUiRow) : MiraEvent
{
    /// <summary>
    /// Gets the <see cref="GameObject"/> parent for the very top UI row.
    /// </summary>
    public GameObject MainTopUiRow { get; } = topUiRow;

    /// <summary>
    /// Gets the <see cref="GameObject"/> parent for the second top UI row.
    /// </summary>
    public GameObject SecondTopUiRow { get; } = secondUiRow;
}
