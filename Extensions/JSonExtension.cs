
using System.Text.Json;
using System.Text.Json.Serialization;

namespace vyg_api_sii.Extensions;

public class IgnoreDeserializeConverter<T> : JsonConverter<T>
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return default;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}

public class SerializeRenameConverter : JsonConverter<string>
{
    private readonly string _newName;

    public SerializeRenameConverter(string newName)
    {
        _newName = newName;
    }

    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetString();
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WritePropertyName(_newName);
        writer.WriteStringValue(value);
    }
}