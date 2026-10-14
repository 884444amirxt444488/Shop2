/*
using System.Text.Json;
using System.Text.Json.Serialization;
public class TrimStringJsonConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) 
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Expected a string value");
        }

        string? Value = reader.GetString();

        return Value?.Trim();
    }
    public override void Write
    (
        Utf8JsonWriter writer,
        string value,
        JsonSerializerOptions options
    )
    {
        writer.WriteStringValue(value);
    }
}
*/

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

public class TrimStringJsonConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Expected a string value");
        }
        
        string? Value = reader.GetString();
        
        return Value.Trim();
    }
    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
    
}





