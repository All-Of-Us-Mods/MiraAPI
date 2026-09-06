using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace MiraAPI.Utilities.Components;

/// <summary>
/// Utility component for arranging objects in a centered layout, similar to <see cref="GridArrange"/>.
/// </summary>
/// <param name="iPtr">The <see cref="IntPtr"/> for the component.</param>
[RegisterInIl2Cpp]
[SuppressMessage("Style", "IDE0051:Remove unused private members", Justification = "Unity Convention.")]
public class CenteredGridArrange(IntPtr iPtr) : MonoBehaviour(iPtr)
{
    /// <summary>
    /// Gets or sets the cell size, which is used for spacing.
    /// </summary>
    public Vector2 CellSize { get; set; }

    /// <summary>
    /// Gets or sets the maximum amount of columns.
    /// </summary>
    public int MaxColumns { get; set; } = 6;

    private List<Transform> cells;
    private static readonly List<Transform> CurrentChildren = [];

    private void Start()
    {
        cells = [];
        GetChildsActive();
        CheckCurrentChildren();
    }

    private void FixedUpdate()
    {
        CheckCurrentChildren();
    }

    private void CheckCurrentChildren()
    {
        GetChildsActive();
        if (cells.SequenceEqual(CurrentChildren))
            return;
        cells.Clear();
        foreach (Transform currentChild in CurrentChildren)
            cells.Add(currentChild);
        ArrangeChilds();
    }

    private void GetChildsActive()
    {
        CurrentChildren.Clear();
        foreach (var obj in transform)
        {
            var child = obj.TryCast<Transform>();
            if (child == null) continue;
            if (child.gameObject.activeSelf)
                CurrentChildren.Add(child);
        }
    }

    private void ArrangeChilds()
    {
        if (cells.Count == 0)
            return;

        int totalRows = Mathf.CeilToInt((float)cells.Count / MaxColumns);
        float totalHeight = (totalRows - 1) * CellSize.y;
        float startY = transform.position.y + totalHeight * 0.5f;

        for (int index = 0; index < cells.Count; ++index)
        {
            int row = index / MaxColumns;
            int rowStartIndex = row * MaxColumns;
            int itemsInRow = Mathf.Min(MaxColumns, cells.Count - rowStartIndex);
            int col = index - rowStartIndex;

            float rowWidth = (itemsInRow - 1) * CellSize.x;
            float startX = transform.position.x - rowWidth * 0.5f;

            float x = startX + col * CellSize.x;
            float y = startY - row * CellSize.y;

            Transform cell = cells[index];
            cell.position = new Vector3(x, y, cell.position.z);
        }
    }
}
