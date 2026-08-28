using System;
using System.Linq;
using Il2CppSystem.Collections.Generic;
using MiraAPI.Networking;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.GameOptions.OptionTypes;

/// <summary>
/// An option for selecting an ingame player. It returns an index of the values list, NOT A PLAYER ID. To get a player out of this, index the <see cref="Values"/> list.
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
    /// <param name="title">The title of the option.</param>
    /// <param name="allowNone">Whether the option includes a none option. If None is selected, value will return -1.</param>
    public ModdedPlayerOption(string title, bool allowNone = true) : base(title, allowNone ? -1 : 0, false)
    {
        Data = ScriptableObject.CreateInstance<PlayerSelectionGameSetting>();
        var data = (PlayerSelectionGameSetting)Data;

        data.Title = StringName;
        data.Type = global::OptionTypes.Player;

        Values = new List<NetworkedPlayerInfo>();
        AllowNone = allowNone;
        Value = DefaultValue;
    }

    /// <inheritdoc />
    public override OptionBehaviour CreateOption(
        ToggleOption toggleOpt,
        NumberOption numberOpt,
        StringOption stringOpt,
        PlayerOption playerOpt,
        Transform container)
    {
        var playerOption = Object.Instantiate(playerOpt, container);
        playerOption.name =
            $"{ParentMod!.OptionsTitleText}.PlayerOption.{TranslationController.Instance.GetString(StringName)}";

        var filteredList = GetFilteredPlayers();
        Values = filteredList.ToIl2CppList();

        playerOption.SetUpFromData(Data, 20);
        playerOption.OnValueChanged = (Il2CppSystem.Action<OptionBehaviour>)ValueChanged;

        playerOption.Title = StringName;
        playerOption.TitleText.text = Title.Translate();
        playerOption.Values = Values;
        playerOption.Value = Value;
        playerOption.playerIndex = filteredList.FindIndex(p => p.PlayerId == Value);

        OptionBehaviour = playerOption;
        ModdedOptionsManager.CreatedPlayerOptions.TryAdd(playerOption, this);

        playerOption.SetValueText();
        playerOption.AdjustButtonsActiveState();

        return playerOption;
    }

    /// <summary>
    /// Returns a player ran through the filter.
    /// </summary>
    /// <returns>A list of filtered players.</returns>
    public System.Collections.Generic.List<NetworkedPlayerInfo> GetFilteredPlayers()
    {
        return [.. GameData.Instance.AllPlayers.ToArray().Where(x => PlayerFilter?.Invoke(x) ?? true)];
    }

    /// <summary>
    /// Returns a player from the value. If <see cref="AllowNone"/> is enabled and none is enabled this will return <see langword="null"/>.
    /// </summary>
    /// <returns>A player or <see langword="null"/> if chosen none.</returns>
    public NetworkedPlayerInfo? GetPlayerValue()
    {
        return Value < 0 ? null : Values[Value];
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
        ModdedOptionsManager.AddSettingsChangeMessage(
            HudManager.Instance.Notifier,
            StringName,
            Data.GetValueString(newValue),
            Configuration.PopUpTextColor,
            Configuration.PopUpIconTmp,
            false);
        if (!OptionBehaviour)
        {
            return;
        }

        if (OptionBehaviour is PlayerOption opt)
        {
            opt.Value = newValue;
        }
    }
}
