using System.Linq;

namespace MiraAPI.Cosmetics;

/// <summary>
/// Used to get access to a CosmeticGroup.
/// </summary>
/// <typeparam name="T">The type.</typeparam>
public static class CosmeticGroupSingleton<T> where T : AbstractCosmeticsGroup
{
    private static T? _instance;

    /// <summary>
    /// Gets instance of the cosmetic group.
    /// </summary>
#pragma warning disable CA1000
    public static T Instance => _instance ??= CustomCosmeticManager.Groups.OfType<T>().Single();
#pragma warning restore CA1000
}
