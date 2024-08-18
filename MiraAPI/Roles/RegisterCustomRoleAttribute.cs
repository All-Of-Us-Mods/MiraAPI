using System;

namespace MiraAPI.Roles;

[AttributeUsage(AttributeTargets.Class)]
public class RegisterCustomRoleAttribute : Attribute
{
    public ushort RoleId { get; }

    public RegisterCustomRoleAttribute()
    {
        RoleId = (ushort)(20 + CustomRoleManager.CustomRoles.Count);
    }
    public RegisterCustomRoleAttribute(ushort roleId)
    {
        RoleId = roleId;
    }
}