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
        GetChildrenActive();
        CheckCurrentChildren();
    }

    private void FixedUpdate()
    {
        CheckCurrentChildren();
    }

    private void CheckCurrentChildren()
    {
        GetChildrenActive();
        if (cells.SequenceEqual(CurrentChildren))
            return;
        cells.Clear();
        foreach (var currentChild in CurrentChildren)
            cells.Add(currentChild);
        ArrangeChildren();
    }

    private void GetChildrenActive()
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

    private void ArrangeChildren()
    {
        if (cells.Count == 0)
            return;

        var totalRows = Mathf.CeilToInt((float)cells.Count / MaxColumns);
        var totalHeight = (totalRows - 1) * CellSize.y;
        var startY = transform.position.y + totalHeight * 0.5f;

        for (var index = 0; index < cells.Count; ++index)
        {
            var row = index / MaxColumns;
            var rowStartIndex = row * MaxColumns;
            var itemsInRow = Mathf.Min(MaxColumns, cells.Count - rowStartIndex);
            var col = index - rowStartIndex;

            var rowWidth = (itemsInRow - 1) * CellSize.x;
            var startX = transform.position.x - rowWidth * 0.5f;

            var x = startX + col * CellSize.x;
            var y = startY - row * CellSize.y;

            var cell = cells[index];
            cell.position = new Vector3(x, y, cell.position.z);
        }
    }
}
