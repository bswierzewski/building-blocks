using System.Collections.Frozen;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace BuildingBlocks.Core.Extensions;

/// <summary>
/// Provides extension methods for working with enum values.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Returns the <see cref="DisplayAttribute"/> value for the enum member, or falls back to the enum value name when no display name is defined.
    /// </summary>
    public static string ToDisplayName<TEnum>(this TEnum value) where TEnum : struct, Enum
        => EnumCache<TEnum>.DisplayNames.TryGetValue(value, out var name)
            ? name
            : value.ToString();

    private static class EnumCache<TEnum> where TEnum : struct, Enum
    {
        public static readonly FrozenDictionary<TEnum, string> DisplayNames = typeof(TEnum)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .ToFrozenDictionary(
                keySelector: field => (TEnum)field.GetValue(null)!,
                elementSelector: field => field.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? field.Name
            );
    }
}
