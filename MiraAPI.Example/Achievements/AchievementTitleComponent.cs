using System;
using System.Linq;
using Il2CppInterop.Runtime.Injection;
using Reactor.Utilities.Attributes;
using TMPro;
using UnityEngine;

namespace MiraAPI.Example.Achievements;

[RegisterInIl2Cpp]
public class AchievementTitleComponent(IntPtr cppPtr) : MonoBehaviour(cppPtr)
{
    private TextMeshPro? _titleText;
    private PlayerControl? _player;
    private PoolablePlayer? _poolablePlayer;
    private string? _lastTitleId;

    static AchievementTitleComponent()
    {
        ClassInjector.RegisterTypeInIl2Cpp<AchievementTitleComponent>();
    }

    private void Awake()
    {
        if (!TryGetComponent(out _player))
        {
            TryGetComponent(out _poolablePlayer);
        }

        var origText = _player
            ? _player.cosmetics.nameText
            : _poolablePlayer?.cosmetics.nameText;

        if (origText == null)
        {
            return;
        }

        var go = new GameObject("AchievementTitle");
        go.layer = gameObject.layer;
        go.transform.SetParent(origText.transform.parent, false);

        _titleText = go.AddComponent<TextMeshPro>();
        _titleText.fontMaterial = origText.fontMaterial;
        _titleText.outlineWidth = 0.2f;
        _titleText.outlineColor = Color.black;
        _titleText.transform.localPosition = new Vector3(0, _player ? 1.1f : 0.75f, -0.01f);
        _titleText.fontSize = _player ? 2.5f : 1.8f;
        _titleText.alignment = TextAlignmentOptions.Center;
    }

    private void Update()
    {
        if (_titleText == null)
        {
            return;
        }

        var playerRef = _player ?? (_poolablePlayer
            ? PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p.PlayerId == (byte)_poolablePlayer.ColorId)
            : null);

        if (playerRef == null)
        {
            return;
        }

        var equipped = MiraAPI.Achievements.AchievementManager.GetEquippedTitle(playerRef);
        var currentId = equipped?.Id;

        if (currentId == _lastTitleId)
        {
            return;
        }

        _lastTitleId = currentId;

        if (equipped != null)
        {
            _titleText.text = $"[{equipped.Title}]";
            _titleText.color = equipped.TitleColor;
        }
        else
        {
            _titleText.text = string.Empty;
        }
    }

    private void OnDestroy()
    {
        if (_titleText != null)
        {
            Destroy(_titleText.gameObject);
        }
    }
}
