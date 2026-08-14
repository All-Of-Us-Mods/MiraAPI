using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using MiraAPI.Modifiers.Types;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using Random = System.Random;

namespace MiraAPI.Modifiers;

/// <summary>
/// The manager for handling <see cref="BaseModifier"/>s.
/// </summary>
public static class ModifierManager
{
    /// <summary>
    /// Gets or sets a value indicating whether <see cref="BaseModifier"/>s should be assigned to players.
    /// </summary>
    public static bool MiraAssignsModifiers { get; set; } = true;

    /// <summary>
    /// Gets the list of all registered <see cref="BaseModifier"/>s.
    /// </summary>
    public static IReadOnlyList<BaseModifier> Modifiers { get; internal set; } = [];

    internal static List<BaseModifier> InternalModifiers { get; } = [];

    private static readonly Dictionary<uint, Type> IdToTypeModifierMap = [];
    private static readonly Dictionary<Type, uint> TypeToIdModifierMap = [];
    private static readonly Dictionary<int, List<uint>> PrioritiesToIdsMap = [];

    private static uint _nextTypeId;

    private static uint GetNextTypeId()
    {
        _nextTypeId++;
        return _nextTypeId;
    }

    /// <summary>
    /// Gets the modifier type from the type id.
    /// </summary>
    /// <param name="id">The ID.</param>
    /// <returns>The Type of the modifier.</returns>
    public static Type? GetModifierType(uint id)
    {
        return IdToTypeModifierMap.GetValueOrDefault(id);
    }

    /// <summary>
    /// Gets the modifier id from the type.
    /// </summary>
    /// <param name="type">The Type.</param>
    /// <returns>The ID of the modifier.</returns>
    public static uint? GetModifierTypeId(Type type)
    {
        return !TypeToIdModifierMap.TryGetValue(type, out var id) ? null : id;
    }

    internal static bool RegisterModifier(Type modifierType, MiraPluginInfo info)
    {
        if (!typeof(BaseModifier).IsAssignableFrom(modifierType))
        {
            return false;
        }

        IdToTypeModifierMap.Add(GetNextTypeId(), modifierType);
        TypeToIdModifierMap.Add(modifierType, _nextTypeId);

        BaseModifier modifier;
        if (modifierType.GetConstructor(Type.EmptyTypes) != null)
        {
            modifier = (BaseModifier)Activator.CreateInstance(modifierType)!;
        }
        else
        {
            // this is probably terrible but its good enough for now.
            modifier = (BaseModifier)FormatterServices.GetUninitializedObject(modifierType);
        }

        info.InternalModifiers.Add(modifier);
        InternalModifiers.Add(modifier);

        if (modifier is not GameModifier gameModifier)
        {
            return true;
        }

        if (modifierType.GetConstructor(Type.EmptyTypes) == null)
        {
            Error($"Game Modifier {modifierType.FullName} does not have a parameterless constructor!");
            return false;
        }

        var priority = gameModifier.Priority();

        if (!PrioritiesToIdsMap.TryGetValue(priority, out var list))
        {
            PrioritiesToIdsMap[priority] = list = [];
        }

        list.Add(_nextTypeId);
        return true;
    }

    /// <summary>
    /// Assigns modifiers to players. ONLY CALL THIS METHOD IF YOU KNOW WHAT YOU ARE DOING.
    /// </summary>
    /// <param name="plrs">The players to assign modifiers to.</param>
    public static void AssignModifiers(List<PlayerControl> plrs)
    {
        var rand = new Random();

        // Filter and sort modifiers by descending priority.
        var modifiers = IdToTypeModifierMap
            .Where(x => x.Value.IsAssignableTo(typeof(GameModifier)))
            .Select(x => Activator.CreateInstance(x.Value) as GameModifier)
            .OfType<GameModifier>()
            .Where(x => x.GetAmountPerGame() > 0 && x.GetAssignmentChance() > 0)
            .Shuffle()
            .OrderByDescending(x => x.Priority())
            .ToArray();

        foreach (var modifier in modifiers)
        {
            var assignments = modifier.GetAmountPerGame();

            var validPlayers = plrs.Where(x => IsGameModifierValid(x, modifier, modifier.TypeId)).ToList();
            if (validPlayers.Count == 0)
            {
                Warning($"No valid players for modifier {modifier.ModifierName}");
                continue;
            }

            assignments = Math.Min(assignments, validPlayers.Count);
            var availablePlayers = new List<PlayerControl>(validPlayers);

            for (var i = 0; i < assignments; i++)
            {
                var chance = Math.Clamp(modifier.GetAssignmentChance(), 0, 100);
                if (rand.Next(100) >= chance)
                {
                    continue;
                }

                var candidates = availablePlayers
                    .Where(x => IsGameModifierPostCheck(x, modifier, modifier.TypeId))
                    .ToList();

                if (candidates.Count == 0)
                {
                    Warning(
                        $"No available players for modifier {modifier.ModifierName} at assignment {i + 1}");
                    break;
                }

                var plr = candidates.Random();
                if (plr == null)
                {
                    Warning($"Valid player for modifier {modifier.ModifierName} disappeared");
                    continue;
                }

                plr.RpcAddModifier(modifier.TypeId);
                availablePlayers.Remove(plr);
            }
        }
    }

    private static bool IsGameModifierValid(PlayerControl player, GameModifier modifier, uint modifierId)
    {
        return (player.Data.Role is not ICustomRole role || role.IsModifierApplicable(modifier)) &&
               !player.HasModifier(modifierId) && modifier.IsModifierValidOn(player.Data.Role) &&
               modifier.CanSpawnOnCurrentMode();
    }
    private static bool IsGameModifierPostCheck(PlayerControl player, GameModifier modifier, uint modifierId)
    {
        return (player.Data.Role is not ICustomRole role || role.IsModifierApplicable(modifier)) &&
               !player.HasModifier(modifierId) && modifier.IsModifierValidOnPostCheck(player.Data.Role);
    }
}
