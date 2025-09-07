using System.Linq;
using HarmonyLib;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace MiraAPI.Hud;

/// <summary>
/// Custom action button that has multiple target objects.
/// </summary>
/// <typeparam name="T">The type of the target objects.</typeparam>
public abstract class MultiTargetButton<T> : CustomActionButton where T : MonoBehaviour
{
    /// <summary>
    /// Gets or sets the list of targets of the button.
    /// </summary>
    public T[] Targets { get; protected set; } = [];

    /// <inheritdoc cref="CustomActionButton{T}.Distance"/>
    public virtual float Distance => PlayerControl.LocalPlayer.Data.Role.GetAbilityDistance();

    /// <summary>
    /// Gets the maximum amount of targets the button can have.
    /// </summary>
    public virtual int MaxTargets { get; } = int.MaxValue;

    /// <inheritdoc cref="CustomActionButton{T}.IsTargetValid"/>
    public virtual bool IsTargetValid(T target)
    {
        return target != null;
    }

    /// <summary>
    /// The method used to get the target objects.
    /// </summary>
    /// <returns>The list of target objects.</returns>
    public abstract T[] GetTargets();

    /// <inheritdoc cref="CustomActionButton{T}.SetOutline"/>
    /// <param name="target">The target object to set the oultine.</param>
    public abstract void SetOutline(T target, bool active);

    /// <inheritdoc cref="CustomActionButton{T}.CanUse"/>
    public override bool CanUse()
    {
        var newTargets = GetTargets();
        foreach (var target in Targets)
        {
            if (!newTargets.Contains(target))
            {
                SetOutline(target, false);
            }
        }

        Targets = newTargets
            .Where(IsTargetValid)
            .Take(MaxTargets)
            .ToArray();
        Targets.Do(t => SetOutline(t, true));

        return base.CanUse() && Targets.Length > 0;
    }

    /// <inheritdoc cref="CustomActionButton{T}.CanClick"/>
    public override bool CanClick()
    {
        return base.CanClick() && Targets.Length > 0;
    }

    /// <summary>
    /// Use this to reset the button's targets after used.
    /// </summary>
    public virtual void ResetTarget()
    {
        Targets.Do(t => SetOutline(t, false));
        Targets = [];
    }
}
