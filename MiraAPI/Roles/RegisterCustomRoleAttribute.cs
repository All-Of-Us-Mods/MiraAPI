using BepInEx.Unity.IL2CPP;
using MiraAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MiraAPI.Roles;

[AttributeUsage(AttributeTargets.Class)]
public class RegisterCustomRoleAttribute(ushort roleId) : Attribute
{ 
    public ushort RoleId { get; } = roleId;

    public RegisterCustomRoleAttribute() : this((ushort)(20 + CustomRoleManager.CustomRoles.Count))
    {
    }
}