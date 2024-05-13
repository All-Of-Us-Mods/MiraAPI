using MiraAPI.Roles;
using Reactor.Utilities.Extensions;
using System.Linq;
using UnityEngine;

namespace MiraAPI.Utilities
{
    public static class Extensions
    {
        public static void UpdateBodies(this PlayerControl playerControl, Color outlineColor, ref DeadBody target)
        {
            foreach (var body in Object.FindObjectsOfType<DeadBody>())
            {
                foreach (var bodyRenderer in body.bodyRenderers)
                {
                    bodyRenderer.SetOutline(null);
                }
            }

            if (playerControl.Data.Role is not ICustomRole { TargetsBodies: true })
            {
                return;
            }

            target = playerControl.NearestDeadBody();
            if (!target)
            {
                return;
            }

            foreach (var renderer in target.bodyRenderers)
            {
                renderer.SetOutline(outlineColor);
            }
        }

        public static DeadBody NearestDeadBody(this PlayerControl playerControl)
        {
            var results = new Il2CppSystem.Collections.Generic.List<Collider2D>();
            Physics2D.OverlapCircle(playerControl.GetTruePosition(), playerControl.MaxReportDistance / 4f, Helpers.Filter, results);
            return results.ToArray()
                .Where(collider2D => collider2D.CompareTag("DeadBody"))
                .Select(collider2D => collider2D.GetComponent<DeadBody>())
                .FirstOrDefault(component => component && !component.Reported);
        }

        public static PlayerControl GetClosestPlayer(this PlayerControl playerControl, bool includeImpostors, float distance)
        {
            PlayerControl result = null;
            if (!ShipStatus.Instance)
            {
                return null;
            }

            var truePosition = playerControl.GetTruePosition();

            foreach (var playerInfo in GameData.Instance.AllPlayers)
            {
                if (playerInfo.Disconnected || playerInfo.PlayerId == playerControl.PlayerId ||
                    playerInfo.IsDead || !includeImpostors && playerInfo.Role.IsImpostor)
                {
                    continue;
                }

                var @object = playerInfo.Object;
                if (!@object)
                {
                    continue;
                }

                var vector = @object.GetTruePosition() - truePosition;
                var magnitude = vector.magnitude;
                if (!(magnitude <= distance) || PhysicsHelpers.AnyNonTriggersBetween(truePosition,
                    vector.normalized,
                    magnitude, LayerMask.GetMask("Ship", "Objects")))
                {
                    continue;
                }

                result = @object;
                distance = magnitude;
            }
            return result;
        }
    }
}
