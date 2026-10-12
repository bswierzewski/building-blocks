using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using BuildingBlocks.Core.Primitives;

namespace BuildingBlocks.Core.Utilities;

/// <summary>
/// The options of a dropdown built from <typeparamref name="TEnum"/>, in the order the values are declared. The label
/// and description come from <see cref="DisplayAttribute"/>; the label falls back to the name. They are built once
/// per enum, on first use.
/// </summary>
public static class EnumOptions<TEnum> where TEnum : struct, Enum
{
    public static IReadOnlyList<EnumOption<TEnum>> Items { get; } = typeof(TEnum)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .OrderBy(field => field.MetadataToken)
        .Select(field =>
        {
            var display = field.GetCustomAttribute<DisplayAttribute>();
            return new EnumOption<TEnum>(
                (TEnum)field.GetValue(null)!,
                display?.GetName() ?? field.Name,
                display?.GetDescription());
        })
        .ToImmutableArray();
}
