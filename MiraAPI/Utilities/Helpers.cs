using MiraAPI.Roles;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MiraAPI.Utilities;

public static class Helpers
{
    public static readonly ContactFilter2D Filter = ContactFilter2D.CreateLegacyFilter(Constants.NotShipMask, float.MinValue, float.MaxValue);
    public static PlainShipRoom GetRoom(Vector3 pos)
    {
        return ShipStatus.Instance.AllRooms.ToList().Find(room => room.roomArea.OverlapPoint(pos));
    }

    public static DeadBody GetBodyById(byte id)
    {
        return Object.FindObjectsOfType<DeadBody>().FirstOrDefault(body => body.ParentId == id);
    }

    public static string GetSuffix(NumberSuffixes suffix)
    {
        return suffix switch
        {
            NumberSuffixes.None => string.Empty,
            NumberSuffixes.Multiplier => "x",
            NumberSuffixes.Seconds => "s",
            _ => string.Empty
        };
    }

    public static StringBuilder CreateForRole(ICustomRole role)
    {
        var taskStringBuilder = new StringBuilder();
        taskStringBuilder.AppendLine($"{role.RoleColor.ToTextColor()}You are a <b>{role.RoleName}.</b></color>");
        taskStringBuilder.Append("<size=70%>");
        taskStringBuilder.AppendLine($"{role.RoleLongDescription}");
        return taskStringBuilder;
    }
}