﻿using System;
using UnityEngine;

namespace MiraAPI.GameOptions;

public interface IModdedOptionGroup
{
    public string GroupName { get; }
    public Func<bool> GroupVisible => () => true;
    public Color GroupColor => Color.clear;
    public Type AdvancedRole => null;
}