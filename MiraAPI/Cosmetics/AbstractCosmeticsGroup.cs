using System;
using System.Collections.Generic;

namespace MiraAPI.Cosmetics;
public abstract class AbstractCosmeticsGroup
{
    public List<HatData> Hats { get; } = [];
    public List<SkinData> Skins { get; } = [];
    public List<VisorData> Visors { get; } = [];
    public List<NamePlateData> Nameplates { get; } = [];
    public abstract string GroupName { get; }
    public virtual Func<bool> GroupVisible => () => true;
    public virtual SortingMode SortingMode { get; } = SortingMode.AlphabeticName;
    public virtual int SortingPriority { get; } = 0; 
}

public enum SortingMode
{
    Priority,
    AlphabeticProductId,
    AlphabeticStoreName,
    AlphabeticName,
}