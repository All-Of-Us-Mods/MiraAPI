using MiraAPI.Roles;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.Utilities;

/// <summary>
/// A class that contains helper methods.
/// </summary>
public static class Helpers
{
    /// <summary>
    /// Creates a ContactFilter2D from a layer mask.
    /// </summary>
    /// <param name="layerMask">The layer mask.</param>
    /// <returns>A new ContactFilter2D that represents the layer mask.</returns>
    public static ContactFilter2D CreateFilter(int layerMask)
    {
        return ContactFilter2D.CreateLegacyFilter(layerMask, float.MinValue, float.MaxValue);
    }

    /// <summary>
    /// Get the room at a specific position.
    /// </summary>
    /// <param name="pos">The position.</param>
    /// <returns>The ship room if its found.</returns>
    public static PlainShipRoom? GetRoom(Vector3 pos)
    {
        return ShipStatus.Instance.AllRooms.ToList().Find(room => room.roomArea.OverlapPoint(pos));
    }

    /// <summary>
    /// Gets a list of dead bodies within a radius.
    /// </summary>
    /// <param name="source">The source location.</param>
    /// <param name="radius">The radius to search in.</param>
    /// <param name="filter">The contact filter.</param>
    /// <returns>A list of dead bodies.</returns>
    public static List<DeadBody> GetNearestDeadBodies(Vector2 source, float radius, ContactFilter2D filter)
    {
        var results = new Il2CppSystem.Collections.Generic.List<Collider2D>();
        Physics2D.OverlapCircle(source, radius, filter, results);
        return results.ToArray()
            .Where(collider2D => collider2D.CompareTag("DeadBody"))
            .Select(collider2D => collider2D.GetComponent<DeadBody>()).ToList();
    }

    /// <summary>
    /// Gets a list of objects within a radius.
    /// </summary>
    /// <param name="source">The source point.</param>
    /// <param name="radius">The radius to search in.</param>
    /// <param name="filter">The contact filter.</param>
    /// <param name="colliderTag">An optional collider tag.</param>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <returns>A list of objects of type T.</returns>
    public static List<T> GetNearestObjectsOfType<T>(Vector2 source, float radius, ContactFilter2D filter, string? colliderTag = null)
        where T : Component
    {
        var results = new Il2CppSystem.Collections.Generic.List<Collider2D>();
        Physics2D.OverlapCircle(source, radius, filter, results);
        return results.ToArray()
            .Where(collider2D => colliderTag == null || collider2D.CompareTag(colliderTag))
            .Select(collider2D => collider2D.GetComponent<T>()).ToList();
    }

    /// <summary>
    /// Gets the closest players to a specific point.
    /// </summary>
    /// <param name="source">The source point.</param>
    /// <param name="radius">The radius to search in.</param>
    /// <param name="ignoreColliders">Whether colliders should be ignored.</param>
    /// <returns>A list of Player Controls in the radius.</returns>
    public static List<PlayerControl> GetClosestPlayersInCircle(
        Vector2 source,
        float radius,
        bool ignoreColliders = true)
    {
        var newList = GetNearestObjectsOfType<PlayerControl>(source, radius, CreateFilter(Constants.NotShipMask));

        if (!ignoreColliders)
        {
            return newList;
        }

        return (from player in newList
            let vector = player.GetTruePosition() - source
            let magnitude = vector.magnitude
            where !PhysicsHelpers.AnyNonTriggersBetween(
                source,
                vector.normalized,
                magnitude,
                Constants.ShipAndObjectsMask)
            select player).ToList();
    }

    /// <summary>
    /// Gets the closest players to a specific player.
    /// </summary>
    /// <param name="source">The source player.</param>
    /// <param name="distance">Distance to search in.</param>
    /// <param name="ignoreColliders">Whether to ignore colliders.</param>
    /// <param name="ignoreSource">Whether to ignore the source player.</param>
    /// <returns>A list of PlayerControls.</returns>
    public static List<PlayerControl> GetClosestPlayers(
        PlayerControl source,
        float distance = 2f,
        bool ignoreColliders = true,
        bool ignoreSource = true)
    {
        if (!ShipStatus.Instance)
        {
            return [];
        }

        var myPos = source.GetTruePosition();
        var players = GetClosestPlayers(myPos, distance, ignoreColliders);

        return ignoreSource ? players.Where(plr => plr.PlayerId != source.PlayerId).ToList() : players;
    }

    /// <summary>
    /// Gets the closest players to a specific point.
    /// </summary>
    /// <param name="source">The source point.</param>
    /// <param name="distance">The distance to search in.</param>
    /// <param name="ignoreColliders">Whether to ignore colliders.</param>
    /// <returns>A list of Player Controls.</returns>
    public static List<PlayerControl> GetClosestPlayers(
        Vector2 source,
        float distance = 2f,
        bool ignoreColliders = true)
    {
        if (!ShipStatus.Instance)
        {
            return [];
        }

        List<PlayerControl> outputList = [];
        outputList.Clear();
        var allPlayers = GameData.Instance.AllPlayers.ToArray().Select(x => x.Object);

        outputList.AddRange(
            from playerControl in allPlayers
            where playerControl && playerControl.Collider.enabled
            let vector = playerControl.GetTruePosition() - source
            let magnitude = vector.magnitude
            where magnitude <= distance && (ignoreColliders || !PhysicsHelpers.AnyNonTriggersBetween(
                source,
                vector.normalized,
                magnitude,
                Constants.ShipAndObjectsMask))
            select playerControl);

        outputList.Sort(
            delegate(PlayerControl a, PlayerControl b)
            {
                var magnitude2 = (a.GetTruePosition() - source).magnitude;
                var magnitude3 = (b.GetTruePosition() - source).magnitude;
                if (magnitude2 > magnitude3)
                {
                    return 1;
                }

                if (magnitude2 < magnitude3)
                {
                    return -1;
                }

                return 0;
            });
        return outputList;
    }

    /// <summary>
    /// Creates a TextMeshPro object with the specified parameters.
    /// </summary>
    /// <param name="name">The name of the object.</param>
    /// <param name="parent">The object parent.</param>
    /// <param name="alignment">The alignment of the TMP object.</param>
    /// <param name="distance">The distance from the edge.</param>
    /// <param name="fontSize">The font size.</param>
    /// <param name="textAlignment">The text alignment.</param>
    /// <returns>A new TMP object.</returns>
    public static TextMeshPro CreateTextLabel(
        string name,
        Transform parent,
        AspectPosition.EdgeAlignments alignment,
        Vector3 distance,
        float fontSize = 2f,
        TextAlignmentOptions textAlignment = TextAlignmentOptions.Center)
    {
        var textObj = new GameObject(name)
        {
            transform =
            {
                parent = parent,
            },
            layer = LayerMask.NameToLayer("UI"),
        };

        var textMeshPro = textObj.AddComponent<TextMeshPro>();
        textMeshPro.fontSize = fontSize;
        textMeshPro.alignment = textAlignment;
        textMeshPro.font = HudManager.Instance.TaskPanel.taskText.font;
        textMeshPro.fontMaterial = HudManager.Instance.TaskPanel.taskText.fontMaterial;

        var aspectPosition = textObj.AddComponent<AspectPosition>();
        aspectPosition.Alignment = alignment;
        aspectPosition.DistanceFromEdge = distance;
        aspectPosition.AdjustPosition();

        return textMeshPro;
    }

    /// <summary>
    /// Gets a DeadBody by its parent ID.
    /// </summary>
    /// <param name="id">The player ID.</param>
    /// <returns>A dead body or null if its not found.</returns>
    public static DeadBody? GetBodyById(byte id)
    {
        return Object.FindObjectsOfType<DeadBody>().FirstOrDefault(body => body.ParentId == id);
    }

    /// <summary>
    /// Gets the suffix for a MiraNumberSuffixes enum.
    /// </summary>
    /// <param name="suffix">The MiraNumberSuffixes enum.</param>
    /// <returns>A suffix based on the enum.</returns>
    public static string GetSuffix(MiraNumberSuffixes suffix)
    {
        return suffix switch
        {
            MiraNumberSuffixes.None => string.Empty,
            MiraNumberSuffixes.Multiplier => "x",
            MiraNumberSuffixes.Seconds => "s",
            MiraNumberSuffixes.Percent => "%",
            _ => string.Empty,
        };
    }

    /// <summary>
    /// Creates a string builder for the Role Tab.
    /// </summary>
    /// <param name="role">The ICustomRole object.</param>
    /// <returns>A StringBuilder.</returns>
    public static StringBuilder CreateForRole(ICustomRole role)
    {
        var taskStringBuilder = new StringBuilder();
        taskStringBuilder.AppendLine(CultureInfo.InvariantCulture, $"{role.RoleColor.ToTextColor()}You are a <b>{role.RoleName}.</b></color>");
        taskStringBuilder.Append("<size=70%>");
        taskStringBuilder.AppendLine(CultureInfo.InvariantCulture, $"{role.RoleLongDescription}");
        return taskStringBuilder;
    }
}
