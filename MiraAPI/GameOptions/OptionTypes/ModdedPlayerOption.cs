using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.GameOptions.OptionTypes;

/// <summary>
/// An option for selecting an in-game player. It returns an index of the values list, NOT A PLAYER ID. To get a player out of this, index the <see cref="Values"/> list.
/// </summary>
public class ModdedPlayerOption : ModdedOption<int>
{
    /// <summary>
    /// Gets or sets the included players.
    /// </summary>
    public List<NetworkedPlayerInfo> Values { get; set; }

    /// <summary>
    /// Gets or sets a filter for the players included in the option.
    /// </summary>
    public Func<NetworkedPlayerInfo, bool>? PlayerFilter { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the option includes a "None" option. If None is selected, value will return -1.
    /// </summary>
    public bool AllowNone { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedPlayerOption"/> class.
    /// </summary>
    /// <param name="groupPrefix">The translation ID prefix of the group containing this option.</param>
    /// <param name="title">The title of the option.</param>
    /// <param name="allowNone">Whether the option includes a none option. If None is selected, value can return -1.</param>
    public ModdedPlayerOption(string groupPrefix, string title, bool allowNone = true)
        : base(groupPrefix, title, allowNone ? -1 : 0, false)
    {
        Values = [];
        AllowNone = allowNone;

        Value = DefaultValue;

        var data = ScriptableObject.CreateInstance<PlayerSelectionGameSetting>();
        data.Title = StringName;
        data.Type = global::OptionTypes.Player;

        Data = data;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedPlayerOption"/> class.
    /// </summary>
    /// <param name="title">The title of the option.</param>
    /// <param name="allowNone">Whether the option includes a none option. If None is selected, value can return -1.</param>
    public ModdedPlayerOption(string title, bool allowNone = true)
        : this(string.Empty, title, allowNone)
    {
    }

    /// <inheritdoc />
    public override OptionBehaviour CreateOption(
        ToggleOption toggleOpt,
        NumberOption numberOpt,
        StringOption stringOpt,
        PlayerOption playerOpt,
        Transform container)
    {
        Values = GetFilteredPlayers();

        var playerOption = Object.Instantiate(playerOpt, container);
        playerOption.name = $"{ParentMod!.OptionsTitleText}.PlayerOption.{Title}";

        playerOption.SetUpFromData(Data, 20);

        playerOption.Title = StringName;
        playerOption.Values = Values.ToIl2CppList();
        playerOption.Value = Value;
        playerOption.playerIndex = Values.FindIndex(p => p.PlayerId == Value);
        playerOption.optionName = Int32OptionNames.Invalid;
        playerOption.OnValueChanged = (Action<OptionBehaviour>)ValueChanged;

        playerOption.SetValueText();
        playerOption.AdjustButtonsActiveState();

        playerOption.gameObject.AddComponent<MiraPlayerOptionComponent>().PlayerOption = this;

        OptionBehaviour = playerOption;

        return playerOption;
    }

    /// <summary>
    /// Returns a player ran through the filter.
    /// </summary>
    /// <returns>A list of filtered players.</returns>
    public List<NetworkedPlayerInfo> GetFilteredPlayers()
    {
        var players = GameData.Instance.AllPlayers.ToArray();

        IEnumerable<NetworkedPlayerInfo> filtered = players;

        if (PlayerFilter != null)
            filtered = players.Where(PlayerFilter);

        return [.. filtered.OrderBy(x => x.PlayerId)];
    }

    /// <summary>
    /// Returns a player from the value. If <see cref="AllowNone"/> is enabled and none is enabled this will return <see langword="null"/>.
    /// </summary>
    /// <returns>A player or <see langword="null"/> if chosen none.</returns>
    public NetworkedPlayerInfo? GetPlayerValue()
    {
        return (uint)Value >= (uint)Values.Count ? null : Values[Value];
    }

    /// <inheritdoc />
    public override float GetFloatData()
    {
        return Value;
    }

    /// <inheritdoc />
    public override NetData GetNetData()
    {
        return new NetData(Id, BitConverter.GetBytes(Value));
    }

    /// <inheritdoc />
    public override void HandleNetData(byte[] data)
    {
        SetValue(BitConverter.ToInt32(data));
    }

    /// <inheritdoc />
    public override int GetValueFromOptionBehaviour(OptionBehaviour optionBehaviour)
    {
        return optionBehaviour.GetInt();
    }

    /// <inheritdoc />
    protected override void OnValueChanged(int newValue)
    {
        if (OptionBehaviour && OptionBehaviour is PlayerOption opt)
        {
            opt.Value = newValue;
        }
    }

    /// <inheritdoc />
    public override string GetDisplayValue()
    {
        var player = GetPlayerValue();
        return !player
            ? TranslationController.Instance.GetString(StringNames.None)
            : player!.PlayerName;
    }
}
