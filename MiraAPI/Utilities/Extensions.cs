using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using HarmonyLib;
using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using MiraAPI.Roles;
using MiraAPI.Voting;
using Reactor.Utilities.Extensions;
using Rewired;
using Rewired.Data;
using TMPro;
using UnityEngine;

namespace MiraAPI.Utilities;

/// <summary>
/// Extension methods for various classes.
/// </summary>
public static class Extensions
{
    internal static NetData GetNetData(this ICustomRole role)
    {
        var count = role.GetCount();
        var chance = role.GetChance();

        if (count == null)
        {
            Error("Couldn't get role count for NetData, defaulting to zero.");
            count = 0;
        }

        if (chance == null)
        {
            Error("Couldn't get role chance for NetData, defaulting to zero.");
            chance = 0;
        }

        return new NetData(
            RoleId.Get(role),
            BitConverter.GetBytes(count.Value).AddRangeToArray(BitConverter.GetBytes(chance.Value)));
    }

    /// <summary>
    /// Used if you override <see cref="Minigame.Close()"/>.
    /// </summary>
    /// <param name="self">The <see cref="Minigame"/>.</param>
    public static void BaseClose(this Minigame self)
    {
        bool isComplete;
        if (self.amClosing == Minigame.CloseState.Closing)
        {
            self.gameObject.Destroy();
            return;
        }
        if (self.CloseSound && Constants.ShouldPlaySfx())
        {
            SoundManager.Instance.PlaySound(self.CloseSound, false, 1f, null);
        }
        if (PlayerControl.LocalPlayer.Data.Role.TeamType == RoleTeamTypes.Crewmate)
        {
            GameManager.Instance.LogicMinigame.OnMinigameClose();
        }
        if (PlayerControl.LocalPlayer)
        {
            PlayerControl.HideCursorTemporarily();
        }
        self.amClosing = Minigame.CloseState.Closing;
        self.logger.Info(string.Concat("Closing minigame ", self.GetType().Name));
        IAnalyticsReporter analytics = DebugAnalytics.Instance.Analytics;
        NetworkedPlayerInfo data = PlayerControl.LocalPlayer.Data;
        TaskTypes taskType = self.TaskType;
        float realtimeSinceStartup = Time.realtimeSinceStartup - self.timeOpened;
        PlayerTask myTask = self.MyTask;
        if (myTask != null)
        {
            isComplete = myTask.IsComplete;
        }
        else
        {
            isComplete = false;
        }
        analytics.MinigameClosed(data, taskType, realtimeSinceStartup, isComplete);
        self.StartCoroutine(self.CoDestroySelf());
    }

    /// <summary>
    /// Sets the cooldown of an <see cref="ActionButton"/> with a formatted string.
    /// </summary>
    /// <param name="button">The <see cref="ActionButton"/> to set the cooldown for.</param>
    /// <param name="timer">The current timer value.</param>
    /// <param name="maxTimer">The maximum timer value.</param>
    /// <param name="format">The format string to use for the timer text.</param>
    public static void SetCooldownFormat(this ActionButton? button, float timer, float maxTimer, string format = "0")
    {
        var num = Mathf.Clamp(timer / maxTimer, 0f, 1f);
        button!.isCoolingDown = num > 0f;
        button.SetCooldownFill(num);
        if (button.isCoolingDown)
        {
            button.cooldownTimerText.text = timer.ToString(format, NumberFormatInfo.InvariantInfo);
            button.cooldownTimerText.gameObject.SetActive(true);
            return;
        }
        button.cooldownTimerText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Sets the fill-up variant of a cooldown <see cref="ActionButton"/> with a formatted string.
    /// </summary>
    /// <param name="button">The <see cref="ActionButton"/> to set the cooldown for.</param>
    /// <param name="timer">The current timer value.</param>
    /// <param name="maxTimer">The maximum timer value.</param>
    /// <param name="format">The format string to use for the timer text.</param>
    public static void SetFillUpFormat(this ActionButton button, float timer, float maxTimer, string format = "0")
    {
        var num = Mathf.Clamp(timer / maxTimer, 0f, 1f);
        button.isCoolingDown = num > 0f;
        if (button.isCoolingDown && timer < 3f)
        {
            button.graphic.transform.localPosition = button.position + (Vector3)UnityEngine.Random.insideUnitCircle * 0.05f;
            button.cooldownTimerText.text = timer.ToString(format, NumberFormatInfo.InvariantInfo);
            button.cooldownTimerText.gameObject.SetActive(true);
        }
        else
        {
            button.graphic.transform.localPosition = button.position;
        }
        button.SetCooldownFill(num);
    }

    /// <summary>
    /// Returns a random element from the specified sequence.
    /// </summary>
    /// <param name="input">
    /// The sequence to select an element from.
    /// </param>
    /// <typeparam name="T">
    /// The type of elements in the sequence.
    /// </typeparam>
    /// <returns>
    /// A randomly selected element from <paramref name="input"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="input"/> is empty.
    /// </exception>
    public static T RandomSnapshot<T>(this IEnumerable<T> input)
    {
        var list = input.ToList();

        if (list.Count == 0)
            throw new InvalidOperationException("Cannot get random element from an empty collection.");

        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    /// <summary>
    /// Gets a <see cref="PlayerControl"/> from their <see cref="PlayerVoteArea"/> in a meeting.
    /// </summary>
    /// <param name="state">The <see cref="PlayerVoteArea"/>.</param>
    /// <returns>The player's <see cref="PlayerControl"/>.</returns>
    public static PlayerControl? GetPlayer(this PlayerVoteArea state) => GameData.Instance.GetPlayerById(state.PlayerId)?.Object;

    /// <summary>
    /// Gets an <see langword="int"/> representing the amount of tasks a player has left.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>A count of how many tasks the player has left.</returns>
    public static int GetTasksLeft(this PlayerControl player) => player.Data.Tasks.ToArray().Count(x => !x.Complete);

    /// <summary>
    /// Checks if a <see cref="PlayerControl"/> is the game's host.
    /// </summary>
    /// <param name="playerControl">The <see cref="PlayerControl"/> you're checking for.</param>
    /// <returns>If the player is the host, <see langword="true"/>, else <see langword="false"/>.</returns>
    public static bool IsHost(this PlayerControl playerControl)
    {
        return TutorialManager.InstanceExists || AmongUsClient.Instance.HostId == playerControl.OwnerId;
    }

    /// <summary>
    /// Checks if a <see cref="PlayerControl"/> is protected by a Guardian Angel.
    /// </summary>
    /// <param name="playerControl">The <see cref="PlayerControl"/> you're checking for.</param>
    /// <returns>If the player is protected, <see langword="true"/>, else <see langword="false"/>.</returns>
    public static bool ProtectedByGa(this PlayerControl playerControl)
    {
        return playerControl.protectedByGuardianId > -1;
    }

    /// <summary>
    /// Used to convert a <see cref="List{T}"/> to <see cref="Il2CppSystem.Collections.Generic.List{T}"/>.
    /// </summary>
    /// <param name="systemList">The <see cref="List{T}"/>.</param>
    /// <typeparam name="T">The type in the list.</typeparam>
    /// <returns>The converted list.</returns>
    public static Il2CppSystem.Collections.Generic.List<T> ToIl2CppList<T>(this List<T> systemList)
    {
        var il2cppList = new Il2CppSystem.Collections.Generic.List<T>();

        foreach (var item in systemList)
        {
            il2cppList.Add(item);
        }

        return il2cppList;
    }

    /// <summary>
    /// Determines if a <see langword="float"/> is an integer.
    /// </summary>
    /// <param name="number">The <see langword="float"/> number.</param>
    /// <returns><see langword="true"/> if the <see langword="float"/> is an integer, <see langword="false"/> otherwise.</returns>
    public static bool IsInteger(this float number)
    {
        return Mathf.Approximately(number, Mathf.Round(number));
    }

    /// <summary>
    /// Destroys the <see cref="GameObject"/> properly.
    /// </summary>
    /// <param name="obj">The <see cref="GameObject"/> to destroy.</param>
    /// <param name="clearGc">Whether to run the garbage collector immediately.</param>
    public static void DeepDestroy(this GameObject obj, bool clearGc = true)
    {
        obj.Destroy();
    }

    /// <summary>
    /// Runs a given action in a loop with a frame budget based on the current target frame rate. Once the budget is hit, the loop continues on the next frame.
    /// </summary>
    /// <param name="collection">The collection to iterate over.</param>
    /// <param name="action">The action to perform on each item.</param>
    /// <typeparam name="T">The type of the collection.</typeparam>
    /// <returns>Coroutine.</returns>
    public static IEnumerator CoLoopWithBudget<T>(this IEnumerable<T> collection, Action<T> action)
    {
        var fps = Application.targetFrameRate > 0 ? Application.targetFrameRate : 60;
        long budget = 1 / (fps * 2); // default is half of current frame time.
        yield return collection.CoLoopWithBudget(budget, action);
    }

    /// <summary>
    /// Runs a given action in a loop with a given frame budget. Once the budget is hit, the loop continues on the next frame.
    /// </summary>
    /// <param name="collection">The collection to iterate over.</param>
    /// <param name="frameBudget">The # of milliseconds to spend in the loop.</param>
    /// <param name="action">The action to perform on each item.</param>
    /// <typeparam name="T">The type of the collection.</typeparam>
    /// <returns>Coroutine.</returns>
    public static IEnumerator CoLoopWithBudget<T>(this IEnumerable<T> collection, long frameBudget, Action<T> action)
    {
        var timer = new Stopwatch();
        timer.Start();
        foreach (var item in collection)
        {
            action(item);

            if (timer.ElapsedMilliseconds > frameBudget)
            {
                timer.Restart();
                yield return null;
            }
        }
    }

    private static IEnumerator Nuke(GameObject? go, bool clearGc)
    {
        if (go == null)
            yield break;

        try
        {
            go.transform.SetParent(null, false);
        }
        catch
        {
            // ignored
        }

        try
        {
            go.SetActive(false);
        }
        catch
        {
            // ignored
        }

        foreach (var mb in go.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (mb == null)
                continue;

            try
            {
                mb.StopAllCoroutines();
            }
            catch
            {
                // ignored
            }

            try
            {
                mb.enabled = false;
            }
            catch
            {
                // ignored
            }
        }

        foreach (var renderer in go.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer == null)
                continue;

            try
            {
                foreach (var mat in renderer.materials)
                {
                    if (mat != null)
                        UnityEngine.Object.Destroy(mat);
                }
            }
            catch
            {
                // ignored
            }
        }

        foreach (var filter in go.GetComponentsInChildren<MeshFilter>(true))
        {
            if (filter == null)
                continue;

            try
            {
                var mesh = filter.mesh;
                if (mesh != null)
                    UnityEngine.Object.Destroy(mesh);
            }
            catch
            {
                // ignored
            }
        }

        UnityEngine.Object.Destroy(go);
        yield return null;
    }

    /// <summary>
    /// Clears up the Garbage Collector manually if necessary.
    /// </summary>
    public static void ClearGarbageCollector()
    {
        Resources.UnloadUnusedAssets();
        Il2CppSystem.GC.Collect();
        GC.Collect();
    }

    /// <summary>
    /// Gets all child objects in a parent object.
    /// </summary>
    /// <returns>An <see cref="IEnumerable"/> that contains <see cref="GameObject"/>s.</returns>
    public static IEnumerable<GameObject> GetAllChildren(this GameObject go)
    {
        for (var i = 0; i < go.transform.childCount; i++)
        {
            yield return go.transform.GetChild(i).gameObject;
        }
    }

    /// <summary>
    /// Gets all child objects in a parent Transform.
    /// </summary>
    /// <returns>An <see cref="IEnumerable"/> that contains <see cref="GameObject"/>s.</returns>
    public static IEnumerable<GameObject> GetAllChildren(this Transform go)
    {
        for (var i = 0; i < go.transform.childCount; i++)
        {
            yield return go.transform.GetChild(i).gameObject;
        }
    }

    /// <summary>
    /// Resizes a <see cref="SpriteRenderer"/> appropriate to the maximum pixel size without messing up ratios.
    /// </summary>
    /// <param name="sprite">The <see cref="SpriteRenderer"/> to adjust.</param>
    /// <param name="pixelSize">The scale for the sprite to be adjusted to.</param>
    public static void SetSizeLimit(this SpriteRenderer sprite, float pixelSize)
    {
        sprite.drawMode = SpriteDrawMode.Sliced;
        if (!sprite.sprite)
        {
            return;
        }

        float spriteWidth = sprite.sprite.rect.width;
        float spriteHeight = sprite.sprite.rect.height;

        if (spriteWidth < spriteHeight)
        {
            sprite.size = new Vector2(pixelSize * spriteWidth / spriteHeight, pixelSize);
        }
        else
        {
            sprite.size = new Vector2(pixelSize, pixelSize * spriteHeight / spriteWidth);
        }
    }

    /// <summary>
    /// Resizes a <see cref="SpriteRenderer"/> appropriate to the maximum pixel size without messing up ratios.
    /// </summary>
    /// <param name="spriteObj">The <see cref="GameObject"/> to adjust.</param>
    /// <param name="pixelSize">The scale for the sprite to be adjusted to.</param>
    public static void SetSizeLimit(this GameObject spriteObj, float pixelSize)
    {
        if (!spriteObj.TryGetComponent<SpriteRenderer>(out var sprite))
        {
            return;
        }

        sprite.SetSizeLimit(pixelSize);
    }

    /// <summary>
    /// Gets a cache of <see cref="PlayerControl"/>'s <see cref="PlayerVoteData"/> to improve performance.
    /// </summary>
    public static Dictionary<PlayerControl, PlayerVoteData> VoteDataComponents { get; } = [];

    /// <summary>
    /// Gets the <see cref="PlayerVoteData"/> of a <see cref="PlayerControl"/>.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> object.</param>
    /// <returns>A <see cref="PlayerVoteData"/> if there is one, <see langword="null"/> otherwise.</returns>
    public static PlayerVoteData GetVoteData(this PlayerControl player)
    {
        if (VoteDataComponents.TryGetValue(player, out var component))
        {
            return component;
        }

        component = player.GetComponent<PlayerVoteData>();
        if (!component)
        {
            throw new InvalidOperationException("PlayerVoteData is not attached to the player.");
        }

        VoteDataComponents[player] = component;
        return component;
    }

    /// <summary>
    /// Gets the maximum value from a <see cref="Dictionary{TKey, TValue}"/> of integers, returning the key and value.
    /// </summary>
    /// <param name="self">The <see cref="Dictionary{TKey, TValue}"/> to search.</param>
    /// <param name="tie">Whether there is a tie for the maximum value.</param>
    /// <returns>The <see cref="KeyValuePair{TKey, TValue}"/> with the maximum value.</returns>
    public static KeyValuePair<byte, int> MaxPair(this Dictionary<byte, int> self, out bool tie)
    {
        tie = true;
        var result = new KeyValuePair<byte, int>(byte.MaxValue, int.MinValue);
        foreach (var keyValuePair in self)
        {
            if (keyValuePair.Value > result.Value)
            {
                result = keyValuePair;
                tie = false;
            }
            else if (keyValuePair.Value == result.Value)
            {
                tie = true;
            }
        }
        return result;
    }

    /// <summary>
    /// Gets the maximum value from a <see cref="Dictionary{TKey, TValue}"/> of floats, returning the key and value.
    /// </summary>
    /// <param name="self">The <see cref="Dictionary{TKey, TValue}"/> to search.</param>
    /// <param name="tie">Whether there is a tie for the maximum value.</param>
    /// <returns>The <see cref="KeyValuePair{TKey, TValue}"/> with the maximum value.</returns>
    public static KeyValuePair<byte, float> MaxPair(this Dictionary<byte, float> self, out bool tie)
    {
        tie = true;
        var result = new KeyValuePair<byte, float>(byte.MaxValue, int.MinValue);
        foreach (var keyValuePair in self)
        {
            if (keyValuePair.Value > result.Value)
            {
                result = keyValuePair;
                tie = false;
            }
            else if (Math.Abs(keyValuePair.Value - result.Value) < .05)
            {
                tie = true;
            }
        }
        return result;
    }

    /// <summary>
    /// Gets the best constructor for a type based on the specified arguments.
    /// </summary>
    /// <param name="type">The type to get the constructor from.</param>
    /// <param name="args">The arguments to pass into the constructor.</param>
    /// <returns>The best constructor.</returns>
    public static ConstructorInfo? GetBestConstructor(this Type type, params object[] args)
    {
        return type.GetValidConstructors(args)
            .OrderBy(
                ctor => ctor.GetParameters()
                    .Select((p, i) => GetInheritanceDistance(args[i].GetType(), p.ParameterType))
                    .Sum())
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets a proper string for an <see cref="Enum"/>. (with spaces).
    /// </summary>
    /// <param name="enum">The <see cref="Enum"/> you would like to change.</param>
    /// <returns>A proper string for the <see cref="Enum"/>.</returns>
    public static string ToDisplayString(this Enum @enum)
    {
        var regex = new Regex(@"([^\^])([A-Z][a-z$])");
        return regex.Replace(@enum.ToString(), m => $"{m.Groups[1].Value} {m.Groups[2].Value}");
    }

    /// <summary>
    /// Gets the constructors of a type that match the specified arguments.
    /// </summary>
    /// <param name="type">The type to get constructors from.</param>
    /// <param name="args">The arguments to pass into the constructor.</param>
    /// <returns>A collection of valid constructors.</returns>
    public static IEnumerable<ConstructorInfo> GetValidConstructors(this Type type, params object[] args)
    {
        return type.GetConstructors().Where(
            x =>
            {
                var parameters = x.GetParameters();
                return parameters.Length == args.Length && Array.TrueForAll(
                    parameters,
                    t => t.ParameterType.IsInstanceOfType(args[t.Position]));
            });
    }

    /// <summary>
    /// Calculates the inheritance distance from the given type to its target base type.
    /// Lower values mean the type is a closer match.
    /// </summary>
    /// <param name="from">The derived type.</param>
    /// <param name="to">The base type.</param>
    /// <returns>The distance between the types.</returns>
    public static int GetInheritanceDistance(Type from, Type to)
    {
        if (!from.IsAssignableFrom(to))
        {
            return int.MaxValue;
        }

        var type = from;
        var distance = 0;
        while (type != null && type != to)
        {
            type = type.BaseType;
            distance++;
        }
        return type == to ? distance : int.MaxValue;
    }

    /// <summary>
    /// Enables stencil masking on a <see cref="TMP_Text"/> object.
    /// </summary>
    /// <param name="text">The <see cref="TMP_Text"/>.</param>
    public static void EnableStencilMasking(this TMP_Text text)
    {
        text.fontMaterial.SetFloat(ShaderID.Stencil, 1);
        text.fontMaterial.SetFloat(ShaderID.StencilComp, 4);
    }

    /// <summary>
    /// Checks if a type is static.
    /// </summary>
    /// <param name="type">The type being checked.</param>
    /// <returns><see langword="true"/> if the type is static, <see langword="false"/> otherwise.</returns>
    public static bool IsStatic(this Type type)
    {
        return type is { IsClass: true, IsAbstract: true, IsSealed: true };
    }

    /// <summary>
    /// Gets a darkened version of a <see cref="Color32"/>.
    /// </summary>
    /// <param name="color">The original <see cref="Color32"/>.</param>
    /// <param name="darknessAmount">A darkness amount between 0 and 255.</param>
    /// <returns>The darkened <see cref="Color32"/>.</returns>
    public static Color32 GetShadowColor(this Color32 color, byte darknessAmount)
    {
        return
            new Color32(
                (byte)Mathf.Clamp(color.r - darknessAmount, 0, 255),
                (byte)Mathf.Clamp(color.g - darknessAmount, 0, 255),
                (byte)Mathf.Clamp(color.b - darknessAmount, 0, 255),
                byte.MaxValue);
    }

    /// <summary>
    /// Truncates a string to a specified length.
    /// </summary>
    /// <param name="value">The original string.</param>
    /// <param name="maxLength">The maximum length.</param>
    /// <param name="truncationSuffix">An option suffix to attach at the end of the truncated string.</param>
    /// <returns>A truncated string of maxLength with the attached suffix.</returns>
    public static string? Truncate(this string? value, int maxLength, string truncationSuffix = "…")
    {
        return value?.Length > maxLength
            ? value[..maxLength] + truncationSuffix
            : value;
    }

    /// <summary>
    /// Chunks a collection of <see cref="NetData"/> into smaller arrays.
    /// </summary>
    /// <param name="dataCollection">An <see cref="IEnumerable{T}"/> of <see cref="NetData"/>s.</param>
    /// <param name="chunkSize">The max chunk size in bytes.</param>
    /// <returns>A <see cref="Queue{T}"/> of <see cref="NetData"/> arrays.</returns>
    public static Queue<NetData[]> ChunkNetData(this IEnumerable<NetData> dataCollection, int chunkSize)
    {
        Queue<NetData[]> chunks = [];
        List<NetData> current = [];

        var count = 0;
        foreach (var netData in dataCollection)
        {
            var length = netData.GetLength();

            if (length > chunkSize)
            {
                Info($"NetData length is greater than chunk size: {length} > {chunkSize}");
                continue;
            }

            if (count + length > chunkSize)
            {
                chunks.Enqueue([.. current]);
                current.Clear();
                count = 0;
            }

            current.Add(netData);
            count += length;
        }

        if (current.Count > 0)
        {
            chunks.Enqueue([.. current]);
        }

        return chunks;
    }

    /// <summary>
    /// Determines if a given <see cref="OptionBehaviour"/> is for a custom option.
    /// </summary>
    /// <param name="optionBehaviour">The <see cref="OptionBehaviour"/> to be tested.</param>
    /// <returns><see langword="true"/> if the <see cref="OptionBehaviour"/> is for a custom options, <see langword="false"/> otherwise.</returns>
    public static bool IsCustom(this OptionBehaviour optionBehaviour)
    {
        return ModdedOptionsManager.ModdedOptions.Values.Any(
            opt => opt.OptionBehaviour && opt.OptionBehaviour == optionBehaviour)
            || optionBehaviour.Equals(GameModeOption.OptionBehaviour);
    }

    /// <summary>
    /// Randomizes a <see cref="List{T}"/>.
    /// </summary>
    /// <param name="list">The <see cref="List{T}"/> object.</param>
    /// <typeparam name="T">The type of object the <paramref name="list"/> contains.</typeparam>
    /// <returns>A randomized <see cref="List{T}"/> made from the original <paramref name="list"/>.</returns>
    public static List<T> Randomize<T>(this List<T> list)
    {
        List<T> randomizedList = [];
        System.Random rnd = new();
        while (list.Count > 0)
        {
            var index = rnd.Next(0, list.Count);
            randomizedList.Add(list[index]);
            list.RemoveAt(index);
        }

        return randomizedList;
    }

    /// <summary>
    /// Darkens a <see cref="Color"/> by a specified amount.
    /// </summary>
    /// <param name="color">The original <see cref="Color"/>.</param>
    /// <param name="amount">A <see langword="float"/> amount between 0 and 1.</param>
    /// <returns>The darkened <see cref="Color"/>.</returns>
    public static Color DarkenColor(this Color color, float amount = 0.45f)
    {
        return new Color(color.r - amount, color.g - amount, color.b - amount);
    }

    /// <summary>
    /// Lightens a <see cref="Color"/> by a specified amount.
    /// </summary>
    /// <param name="color">The original <see cref="Color"/>.</param>
    /// <param name="amount">A <see langword="float"/> amount between 0.0 and 1.0.</param>
    /// <returns>The lightened <see cref="Color"/>.</returns>
    public static Color LightenColor(this Color color, float amount = 0.45f)
    {
        return new Color(color.r + amount, color.g + amount, color.b + amount);
    }

    /// <summary>
    /// Gets the nearest <see cref="DeadBody"/> to a <see cref="PlayerControl"/>.
    /// </summary>
    /// <param name="playerControl">The <see cref="PlayerControl"/> object.</param>
    /// <param name="radius">The radius to search within.</param>
    /// <returns>The <see cref="DeadBody"/> if it is found, or <see langword="null"/> there is none within the radius.</returns>
    public static DeadBody? GetNearestDeadBody(this PlayerControl playerControl, float radius)
    {
        return Helpers
            .GetNearestDeadBodies(playerControl.GetTruePosition(), radius, Helpers.CreateFilter(Constants.NotShipMask))
            .Find(component => component && !component.Reported);
    }

    /// <summary>
    /// Finds the nearest <typeparamref name="T"/> to a <see cref="PlayerControl"/>. It will only work if the object has a collider.
    /// </summary>
    /// <param name="playerControl">The <see cref="PlayerControl"/> object.</param>
    /// <param name="radius">The radius to search within.</param>
    /// <param name="filter">The contact filter.</param>
    /// <param name="colliderTag">An optional collider tag.</param>
    /// <param name="predicate">Optional predicate to test if the <typeparamref name="T"/> is valid.</param>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <returns>The <typeparamref name="T"/> if it was found, or <see langword="null"/> if there is none within the radius.</returns>
    public static T? GetNearestObjectOfType<T>(
        this PlayerControl playerControl,
        float radius,
        ContactFilter2D filter,
        string? colliderTag = null,
        Predicate<T>? predicate = null) where T : Component
    {
        return Helpers.GetNearestObjectsOfType<T>(playerControl.GetTruePosition(), radius, filter, colliderTag)
            .Find(predicate ?? (component => component));
    }

    /// <summary>
    /// Gets the closest <see cref="PlayerControl"/> that matches the given criteria.
    /// </summary>
    /// <param name="playerControl">The <see cref="PlayerControl"/> object.</param>
    /// <param name="includeImpostors">Whether impostors should be included in the search.</param>
    /// <param name="distance">The radius to search within.</param>
    /// <param name="ignoreColliders">Whether colliders should be ignored when searching.</param>
    /// <param name="includeGhosts">Determines if Ghosts are included.</param>
    /// <param name="predicate">Optional predicate to test if the <see cref="PlayerControl"/> is valid.</param>
    /// <returns>The closest <see cref="PlayerControl"/> if there is one, <see langword="false"/> otherwise.</returns>
    public static PlayerControl? GetClosestPlayer(
        this PlayerControl playerControl,
        bool includeImpostors,
        float distance,
        bool ignoreColliders = false,
        bool includeGhosts = false,
        Predicate<PlayerControl>? predicate = null)
    {
        var filteredPlayers = Helpers.GetClosestPlayers(playerControl, distance, ignoreColliders)
            .Where(
                playerInfo => !playerInfo.Data.Disconnected &&
                              playerInfo.PlayerId != playerControl.PlayerId &&
                              (includeGhosts || !playerInfo.Data.IsDead) &&
                              (includeImpostors || !playerInfo.Data.Role.IsImpostor))
            .ToList();
        return predicate != null ? filteredPlayers.Find(predicate) : filteredPlayers.FirstOrDefault();
    }

    /// <summary>
    /// Fixed version of <see cref="Reactor.Utilities.Extensions.UnityExtensions.SetOutline(Renderer, Color?)"/>.
    /// </summary>
    /// <param name="renderer">The <see cref="Renderer"/> you want to update the outline for.</param>
    /// <param name="color">The outline <see cref="Color"/>.</param>
    public static void UpdateOutline(this Renderer renderer, Color? color)
    {
        renderer.material.SetFloat(ShaderID.Outline, color.HasValue ? 1 : 0);
        renderer.material.SetColor(ShaderID.OutlineColor, color ?? Color.clear);
        renderer.material.SetColor(ShaderID.AddColor, color ?? Color.clear);
    }

    // Inspired by: https://github.com/eDonnes124/Town-Of-Us-R/blob/master/source/Patches/Keybinds.cs#L29

    /// <summary>
    /// Registers a new mod keybind as a user-assignable button action in Rewired.
    /// </summary>
    /// <param name="userData">The Rewired <see cref="UserData"/> to add the action to.</param>
    /// <param name="id">The internal name of the action.</param>
    /// <param name="name">Text shown in the rebinding UI.</param>
    /// /// <param name="group">Group shown above the label.</param>
    /// <param name="key">The default key to assign to this action.</param>
    /// <param name="category">Category ID to group actions in Rewired (default is 0).</param>
    /// <param name="elementIdentifierId">The element identifier ID (default is -1, meaning none specified).</param>
    /// <param name="type">The <see cref="InputActionType"/> for this action (default is Button).</param>
    /// <param name="modifiers">Optional modifier keys (e.g., <c>Control</c>, <c>Shift</c>, <c>Alt</c>) that must be held together with the main key.</param>
    /// <returns>The action ID of the newly registered action.</returns>
    public static InputAction RegisterModBind(this UserData userData, string id, string name, string? group, KeyboardKeyCode key, int category = 0, int elementIdentifierId = -1, InputActionType type = InputActionType.Button, ModifierKey[]? modifiers = null)
    {
        userData.AddAction(category);
        var action = userData.GetAction(userData.actions.Count - 1)!;

        action.name = id;
        action.descriptiveName = group != null
            ? $"<b><size=70%>{Palette.CrewmateRoleHeaderDarkBlue.ToTextColor()}{group.ReplaceLineEndings(" ")}</color></size></b>\n{name}"
            : name;
        action.categoryId = category;
        action.type = type;
        action.userAssignable = true;

        var map = new ActionElementMap
        {
            _elementIdentifierId = elementIdentifierId,
            _actionId = action.id,
            _elementType = ControllerElementType.Button,
            _axisContribution = Pole.Positive,
            _keyboardKeyCode = key,
        };

        if (modifiers != null)
        {
            if (modifiers.Length > 0) map._modifierKey1 = modifiers[0];
            if (modifiers.Length > 1) map._modifierKey2 = modifiers[1];
            if (modifiers.Length > 2) map._modifierKey3 = modifiers[2];
        }

        userData.keyboardMaps[0].actionElementMaps.Add(map);
        userData.joystickMaps[0].actionElementMaps.Add(map);
        return action;
    }
}
