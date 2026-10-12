namespace BuildingBlocks.Core.Primitives;

/// <summary>
/// One option of a dropdown built from an enum: the enum value to send back to the API, its label and an optional
/// description. The value keeps the enum type, so generated clients can pass it straight into a request.
/// </summary>
public sealed record EnumOption<TEnum>(TEnum Value, string Label, string? Description) where TEnum : struct, Enum;
