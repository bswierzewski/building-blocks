using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace BuildingBlocks.Core.Extensions;

/// <summary>
/// Provides extension methods for XML serialization and deserialization.
/// </summary>
public static class XmlExtensions
{
    /// <summary>
    /// Serializes the specified object to its XML representation without default namespaces and with UTF-8 encoding.
    /// </summary>
    public static string ToXml<T>(this T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        using var writer = new Utf8StringWriter();
        XmlCache<T>.Serializer.Serialize(writer, value, XmlCache<T>.EmptyNamespaces);

        return writer.ToString();
    }

    /// <summary>
    /// Deserializes the specified XML string to an object of type <typeparamref name="T"/>.
    /// </summary>
    public static T? FromXml<T>(this string xml)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(xml);

        using var reader = new StringReader(xml);

        return XmlCache<T>.Serializer.Deserialize(reader) is T result
          ? result
          : throw new InvalidOperationException($"Nie udało się zdeserializować XML do typu {typeof(T).FullName}.");
    }

    /// <summary>
    /// Provides a CLR-generated cache for each closed generic type used during XML serialization.
    /// </summary>
    private static class XmlCache<T>
    {
        public static readonly XmlSerializer Serializer = new(typeof(T));

        // Empty namespaces keep the generated XML free from the default xsi/xsd namespace noise.
        public static readonly XmlSerializerNamespaces EmptyNamespaces = new([new XmlQualifiedName(string.Empty, string.Empty)]);
    }

    /// <summary>
    /// Forces the XML declaration to use UTF-8 when the serializer writes to a string.
    /// </summary>
    private sealed class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
