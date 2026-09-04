using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using Rewired;
using TMPro;
using UnityEngine;
using MethodBase = System.Reflection.MethodBase;
using Object = UnityEngine.Object;

namespace MiraAPI.Utilities;

/// <summary>
/// A class that contains helper methods.
/// </summary>
public static class Helpers
{
    /// <summary>
    /// Get all living players.
    /// </summary>
    /// <returns>A list of alive players.</returns>
    public static List<PlayerControl> GetAlivePlayers()
    {
        return [.. GameData.Instance.AllPlayers.ToArray().Where(x => !x.IsDead && !x.Disconnected && x.Object).Select(x => x.Object)];
    }

    internal static GameObject CreateKeybindIcon(GameObject button, KeyboardKeyCode keyCode, Vector3 localPos)
    {
        var keybindIcon = Object.Instantiate(HudManager.Instance.AbilityButton.usesRemainingSprite.gameObject, button.transform);
        keybindIcon.GetComponent<SpriteRenderer>().sprite = MiraAssets.KeybindButton.LoadAsset();
        keybindIcon.transform.GetComponentInChildren<TextMeshPro>().text = keyCode.ToString();
        keybindIcon.name = "KeybindIcon";
        keybindIcon.transform.localPosition = localPos;
        return keybindIcon;
    }

    /// <summary>
    /// Gets the move next of an <see cref="IEnumerator"/> on <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Where the <see cref="Scroller"/> should be parented to.</typeparam>
    /// <param name="methodName">The name of the method being searched for.</param>
    /// <returns>THe corresponding method to patch.</returns>
    public static MethodBase? GetStateMachineMoveNext<T>(string methodName)
    {
        var typeName = typeof(T).FullName;
        var showRoleStateMachine =
            typeof(T)
                .GetNestedTypes()
                .FirstOrDefault(x => x.Name.Contains(methodName));

        if (showRoleStateMachine == null)
        {
            Error($"Failed to find {methodName} state machine for {typeName}");
            return null;
        }

        var moveNext = AccessTools.Method(showRoleStateMachine, "MoveNext");
        if (moveNext == null)
        {
            Error($"Failed to find MoveNext method for {typeName}.{methodName}");
            return null;
        }

        Info($"Found {methodName}.MoveNext");
        return moveNext;
    }

    /// <summary>
    /// Creates a draggable <see cref="Scroller"/>. Add items into the <see cref="Scroller.Inner"/> transform. This does not automatically position children.
    /// </summary>
    /// <param name="parent">Where the <see cref="Scroller"/> should be parented to.</param>
    /// <param name="hitBoxCollider">The collider of the <see cref="Scroller"/>. Used to drag.</param>
    /// <returns>The created <see cref="Scroller"/> object.</returns>
    public static Scroller CreateScroller(Transform parent, BoxCollider2D hitBoxCollider)
    {
        var scrollObj = new GameObject("Scroller");
        scrollObj.transform.SetParent(parent);
        scrollObj.transform.localScale = new Vector3(1, 1, 1);

        var inner = new GameObject("Inner");
        inner.transform.SetParent(scrollObj.transform);
        inner.transform.localScale = new Vector3(1, 1, 1);
        inner.transform.localPosition = new Vector3(0, 0, 2);

        var scroller = scrollObj.AddComponent<Scroller>();
        scroller.allowX = false;
        scroller.allowY = true;
        scroller.DragScrollSpeed = 1f;
        scroller.Colliders = new Il2CppReferenceArray<Collider2D>([hitBoxCollider]);
        scroller.Inner = inner.transform;

        return scroller;
    }

    /// <summary>
    /// Divides a button/etc by a certain amount by resizing colliders and renderer sizes.
    /// </summary>
    /// <param name="obj">The <see cref="GameObject"/> you want to divide.</param>
    /// <param name="amount">How much you want to divide by.</param>
    public static void DivideSize(GameObject obj, float amount)
    {
        foreach (var collider in obj.GetComponentsInChildren<Collider2D>(true))
        {
            if (collider.TryCast<BoxCollider2D>() is { } col)
            {
                col.size = new Vector2(col.size.x / amount, col.size.y);
            }
        }

        foreach (var rend in obj.GetComponentsInChildren<SpriteRenderer>(true))
        {
            rend.size = new Vector2(rend.size.x / amount, rend.size.y);
        }
    }

    /// <summary>
    /// Gets the keybind for an action with ReInput.
    /// </summary>
    /// <param name="actionId">The action ID.</param>
    /// <returns>The keyboard key code.</returns>
    public static KeyboardKeyCode GetKeybindByActionId(int actionId)
    {
        var player = ReInput.players.GetPlayer(0);
        return player.controllers.maps.GetFirstElementMapWithAction(ControllerType.Keyboard, actionId, false)
            .keyboardKeyCode;
    }

    /// <summary>
    /// Determines whether a given probability check succeeds.
    /// </summary>
    /// <param name="probability">An integer value representing the success probability (0-100).</param>
    /// <returns><see langword="true"/> if the number falls in the range, <see langword="false"/> if not.</returns>
    public static bool CheckChance(int probability)
    {
        switch (probability)
        {
            case 0:
                return false;
            case 100:
                return true;
            default:
                {
                    var num = Random.RandomRangeInt(1, 101);
                    return num <= probability;
                }
        }
    }

    /// <summary>
    /// Retrieves a <see cref="Vent"/> from the ID.
    /// </summary>
    /// <param name="id">The vent ID.</param>
    /// <returns>The <see cref="Vent"/>.</returns>
    public static Vent? GetVentById(int id)
    {
        return ShipStatus.Instance.AllVents.FirstOrDefault(vent => vent.Id == id);
    }

    /// <summary>
    /// Creates an <see cref="ArrowBehaviour"/>.
    /// </summary>
    /// <param name="parent">The arrow <see cref="GameObject"/>'s parent.</param>
    /// <param name="color">The color of the arrow.</param>
    /// <returns>The created <see cref="ArrowBehaviour"/>.</returns>
    public static ArrowBehaviour CreateArrow(Transform parent, Color color)
    {
        var prefab = Object.FindObjectOfType<ArrowBehaviour>(true);
        var arrow = Object.Instantiate(prefab, parent);
        arrow.image = arrow.gameObject.GetComponent<SpriteRenderer>();
        arrow.image.color = color;
        arrow.gameObject.layer = 5;
        arrow.gameObject.SetActive(true);
        return arrow;
    }

    /// <summary>
    /// Get the closest <typeparamref name="T"/>.
    /// </summary>
    /// <param name="objectList">A list of all <typeparamref name="T"/>s you'd like to check the distance for.</param>
    /// <param name="position">The position of where you want to check from. For example: <c>PlayerControl.LocalPlayer.transform.position</c>.</param>
    /// <typeparam name="T">The object type.</typeparam>
    /// <returns>The closest object.</returns>
    public static T? FindClosestObjectOfType<T>(List<T> objectList, Vector3 position) where T : MonoBehaviour
    {
        T? closest = null;
        var closestDistanceSqr = Mathf.Infinity;

        foreach (var obj in objectList)
        {
            if (obj == null)
            {
                continue;
            }

            var sqrDistance = (obj.transform.position - position).sqrMagnitude;
            if (sqrDistance < closestDistanceSqr)
            {
                closestDistanceSqr = sqrDistance;
                closest = obj;
            }
        }

        return closest;
    }

    /// <summary>
    /// Creates and shows a notification.
    /// </summary>
    /// <param name="text">The text you want to display.</param>
    /// <param name="color">The <see cref="Color"/> of the text and image.</param>
    /// <param name="clip">The sound you want to play with the notification.</param>
    /// <param name="spr">The <see cref="Sprite"/> beside the notification.</param>
    /// <returns>The created notification.</returns>
    public static LobbyNotificationMessage CreateAndShowNotification(string text, Color color, AudioClip? clip = null, Sprite? spr = null)
    {
        return CreateAndShowNotification(text, color, new Vector3(0f, 0f, -2f), clip, spr);
    }

    /// <summary>
    /// Creates and shows a notification.
    /// </summary>
    /// <param name="text">The text you want to display.</param>
    /// <param name="color">The <see cref="Color"/> of the text and image.</param>
    /// <param name="localPos">The position of the notification.</param>
    /// <param name="clip">The sound you want to play with the notification.</param>
    /// <param name="spr">The <see cref="Sprite"/> beside the notification.</param>
    /// <returns>The created notification.</returns>
    public static LobbyNotificationMessage CreateAndShowNotification(string text, Color color, Vector3 localPos, AudioClip? clip = null, Sprite? spr = null)
    {
        var popper = HudManager.Instance.Notifier;
        var newMessage = Object.Instantiate(popper.notificationMessageOrigin, Vector3.zero, Quaternion.identity, popper.transform);
        newMessage.transform.localPosition = localPos;
        newMessage.SetUp(text, spr ?? null, color, new System.Action(() => popper.OnMessageDestroy(newMessage)));
        popper.lastMessageKey = -1;
        popper.ShiftMessages();
        popper.AddMessageToQueue(newMessage);

        if (clip == null) return newMessage;
        SoundManager.Instance.StopSound(clip);
        SoundManager.Instance.PlaySound(clip, false, 2f);

        return newMessage;
    }

    /// <summary>
    /// Returns an empty coroutine.
    /// </summary>
    /// <returns>Empty coroutine.</returns>
    public static IEnumerator EmptyCoroutine()
    {
        yield break;
    }

    /// <summary>
    /// Creates a <see cref="ContactFilter2D"/> from a layer mask.
    /// </summary>
    /// <param name="layerMask">The layer mask.</param>
    /// <returns>A new <see cref="ContactFilter2D"/> that represents the layer mask.</returns>
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
        return ShipStatus.Instance.AllRooms.FirstOrDefault(room => room.roomArea.OverlapPoint(pos));
    }

    /// <summary>
    /// Gets a list of <see cref="DeadBody"/>s within a radius.
    /// </summary>
    /// <param name="source">The source location.</param>
    /// <param name="radius">The radius to search in.</param>
    /// <param name="filter">The contact filter.</param>
    /// <returns>A <see cref="List{T}"/> of <see cref="DeadBody"/>s.</returns>
    public static List<DeadBody> GetNearestDeadBodies(Vector2 source, float radius, ContactFilter2D filter)
    {
        var results = new Il2CppSystem.Collections.Generic.List<Collider2D>();
        Physics2D.OverlapCircle(source, radius, filter, results);
        return [.. results.ToArray()
            .Where(collider2D => collider2D.CompareTag("DeadBody"))
            .Select(collider2D => collider2D.GetComponent<DeadBody>())];
    }

    /// <summary>
    /// Gets a list of <typeparamref name="T"/> within a radius.
    /// </summary>
    /// <param name="source">The source point.</param>
    /// <param name="radius">The radius to search in.</param>
    /// <param name="filter">The contact filter.</param>
    /// <param name="colliderTag">An optional collider tag.</param>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <returns>A <see cref="List{T}"/> of <typeparamref name="T"/>s.</returns>
    public static List<T> GetNearestObjectsOfType<T>(Vector2 source, float radius, ContactFilter2D filter, string? colliderTag = null)
        where T : Component
    {
        var results = new Il2CppSystem.Collections.Generic.List<Collider2D>();
        Physics2D.OverlapCircle(source, radius, filter, results);
        return [.. results.ToArray()
            .Where(collider2D => colliderTag == null || collider2D.CompareTag(colliderTag))
            .Select(collider2D => collider2D.GetComponent<T>())];
    }

    /// <summary>
    /// Gets the closest <see cref="PlayerControl"/>s to a specific point.
    /// </summary>
    /// <param name="source">The source point.</param>
    /// <param name="radius">The radius to search in.</param>
    /// <param name="ignoreColliders">Whether colliders should be ignored.</param>
    /// <returns>A <see cref="List{T}"/> of <see cref="PlayerControl"/>s in the radius.</returns>
    public static List<PlayerControl> GetClosestPlayersInCircle(
        Vector2 source,
        float radius,
        bool ignoreColliders = true)
    {
        var newList = GetNearestObjectsOfType<PlayerControl>(source, radius, CreateFilter(Constants.NotShipMask));

        return !ignoreColliders
            ? newList
            : [.. from player in newList
                   let vector = player.GetTruePosition() - source
                   let magnitude = vector.magnitude
                   where !PhysicsHelpers.AnyNonTriggersBetween(
                        source,
                        vector.normalized,
                        magnitude,
                        Constants.ShipAndObjectsMask)
                   select player];
    }

    /// <summary>
    /// Gets the closest <see cref="PlayerControl"/>s to a specific <see cref="PlayerControl"/>.
    /// </summary>
    /// <param name="source">The source <see cref="PlayerControl"/>.</param>
    /// <param name="distance">Distance to search in.</param>
    /// <param name="ignoreColliders">Whether to ignore colliders.</param>
    /// <param name="ignoreSource">Whether to ignore the <paramref name="source"/> player.</param>
    /// <returns>A <see cref="List{T}"/> of <see cref="PlayerControl"/>s.</returns>
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

        return ignoreSource ? [.. players.Where(plr => plr.PlayerId != source.PlayerId)] : players;
    }

    /// <summary>
    /// Gets the closest <see cref="PlayerControl"/>s to a specific point.
    /// </summary>
    /// <param name="source">The source point.</param>
    /// <param name="distance">The distance to search in.</param>
    /// <param name="ignoreColliders">Whether to ignore colliders.</param>
    /// <returns>A <see cref="List{T}"/> of <see cref="PlayerControl"/>s.</returns>
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
            delegate (PlayerControl a, PlayerControl b)
            {
                var magnitude2 = (a.GetTruePosition() - source).magnitude;
                var magnitude3 = (b.GetTruePosition() - source).magnitude;
                return magnitude2 > magnitude3
                    ? 1
                    : magnitude2 < magnitude3
                        ? -1
                        : 0;
            });
        return outputList;
    }

    /// <summary>
    /// Creates a <see cref="TextMeshPro"/> with the specified parameters.
    /// </summary>
    /// <param name="name">The name of the object.</param>
    /// <param name="parent">The object's parent.</param>
    /// <param name="alignment">The alignment of the <see cref="TextMeshPro"/>.</param>
    /// <param name="distance">The distance from the edge.</param>
    /// <param name="fontSize">The font size.</param>
    /// <param name="textAlignment">The text alignment.</param>
    /// <returns>A new <see cref="TextMeshPro"/>.</returns>
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
    /// Gets a <see cref="DeadBody"/> by its parent ID.
    /// </summary>
    /// <param name="id">The player ID.</param>
    /// <returns>A <see cref="DeadBody"/> or <see langword="null"/> if its not found.</returns>
    public static DeadBody? GetBodyById(byte id)
    {
        return Object.FindObjectsOfType<DeadBody>().FirstOrDefault(body => body.ParentId == id);
    }

    /// <summary>
    /// Gets the suffix for a <see cref="MiraNumberSuffixes"/> <see langword="enum"/>.
    /// </summary>
    /// <param name="suffix">The <see cref="MiraNumberSuffixes"/> <see langword="enum"/>.</param>
    /// <returns>A suffix based on the <see langword="enum"/>.</returns>
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
    /// Converts the first letter of a string to uppercase.
    /// </summary>
    /// <param name="str">The string.</param>
    /// <returns>The fixed string.</returns>
    public static string FirstLetterToUpper(string str)
    {
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Gets a random string based on characters.
    /// </summary>
    /// <param name="length">The length of the string.</param>
    /// <param name="chars">The characters in the random string.</param>
    /// <returns>The random string.</returns>
    public static string RandomString(int length, string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")
    {
        return new string([.. Enumerable.Repeat(chars, length).Select(s => s[Random.RandomRangeInt(0, s.Length)])]);
    }

    /// <summary>
    /// Returns the formatted value using the specified suffix and format string.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <param name="suffix">The suffix to add.</param>
    /// <param name="formatString">The format string to use to format.</param>
    /// <returns>The formatted value.</returns>
    public static string FormatValue(float value, MiraNumberSuffixes suffix = MiraNumberSuffixes.None, string formatString = "0.0")
    {
        return suffix switch
        {
            MiraNumberSuffixes.None => value.ToString(formatString, NumberFormatInfo.InvariantInfo),
            MiraNumberSuffixes.Multiplier => value.ToString(formatString, NumberFormatInfo.InvariantInfo) + "x",
            MiraNumberSuffixes.Percent => value.ToString(formatString, NumberFormatInfo.InvariantInfo) + "%",
            _ => TranslationController.Instance.GetString(
                StringNames.GameSecondsAbbrev,
                (Il2CppSystem.Object[])[value.ToString(formatString, CultureInfo.InvariantCulture)]),
        };
    }

    /// <summary>
    /// Returns the string of a <see cref="RoleBehaviour"/>.
    /// </summary>
    /// <param name="role">The <see cref="RoleBehaviour"/> to find.</param>
    /// <returns>The <see cref="RoleBehaviour"/>'s name.</returns>
    public static string GetRoleName(this RoleBehaviour role)
    {
        return role is ICustomRole custom ? custom.RoleName : role.NiceName;
    }

    /// <summary>
    /// Returns the string of a <see cref="RoleBehaviour"/>.
    /// </summary>
    /// <param name="role">The <see cref="RoleBehaviour"/> to find.</param>
    /// <returns>The <see cref="RoleBehaviour"/>'s intro blurb.</returns>
    public static string GetRoleIntroBlurb(this RoleBehaviour role)
    {
        if (role is ICustomRole custom)
        {
            return custom.RoleDescription;
        }
        return role.Blurb;
    }

    /// <summary>
    /// Returns the string of a <see cref="RoleBehaviour"/>.
    /// </summary>
    /// <param name="role">The <see cref="RoleBehaviour"/> to find.</param>
    /// <returns>The <see cref="RoleBehaviour"/>'s medium description.</returns>
    public static string GetRoleMedDescription(this RoleBehaviour role)
    {
        if (role is ICustomRole custom)
        {
            return custom.RoleDescription;
        }
        return role.BlurbMed;
    }

    /// <summary>
    /// Returns the string of a <see cref="RoleBehaviour"/>.
    /// </summary>
    /// <param name="role">The <see cref="RoleBehaviour"/> to find.</param>
    /// <returns>The <see cref="RoleBehaviour"/>'s long description.</returns>
    public static string GetRoleLongDescription(this RoleBehaviour role)
    {
        if (role is ICustomRole custom)
        {
            return custom.RoleLongDescription;
        }
        return role.BlurbLong;
    }

    /// <summary>
    /// Returns whether the <see cref="RoleBehaviour"/> is blacklisted from appearing and spawning. (Only applicable to vanilla roles).
    /// </summary>
    /// <param name="role">The <see cref="RoleBehaviour"/> to check.</param>
    /// <returns>The <see cref="RoleBehaviour"/>'s blacklist status.</returns>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Intentionally unused because the main intent is for it to be used by dependent mods to add to a blacklist.")]
    public static bool IsRoleBlacklisted(this RoleBehaviour role)
    {
        // This should be patchable by mods when a vanilla role is meant to be replaced by a custom role.
        return false;
    }
}
