/*
using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;


public class JsonTrimModel : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException(" Value not string");
        }
        string? Value = reader.GetString();

        return Value.Trim();
    }
    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
*/

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;

public class JsonTrimModel : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Not string");
        }
        string? Value = reader.GetString();

        return Value.Trim();
    }
    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}



