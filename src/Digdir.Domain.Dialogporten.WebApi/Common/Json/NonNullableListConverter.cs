using System.Text.Json;
using System.Text.Json.Serialization;

namespace Digdir.Domain.Dialogporten.WebApi.Common.Json;

public class NonNullableListConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert.IsGenericType &&
           typeToConvert.GetGenericTypeDefinition() == typeof(List<>);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var elementType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(NonNullableListConverter<>).MakeGenericType(elementType);

        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    private sealed class NonNullableListConverter<T> : JsonConverter<List<T>>
    {
        public override List<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                throw new JsonException("Custom error message: The list cannot be null.");
            }

            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException();
            }

            List<T> list = [];
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    return list;
                }

                // // Get the key.
                // if (reader.TokenType != JsonTokenType.PropertyName)
                // {
                //     throw new JsonException();
                // }
                //
                // string? propertyName = reader.GetString();
                //
                // // For performance, parse with ignoreCase:false first.
                // if (!Enum.TryParse(propertyName, ignoreCase: false, out TKey key) &&
                //     !Enum.TryParse(propertyName, ignoreCase: true, out key))
                // {
                //     throw new JsonException(
                //         $"Unable to convert \"{propertyName}\" to Enum \"{_keyType}\".");
                // }
                //
                // // Get the value.
                // reader.Read();
                // TValue value = _valueConverter.Read(ref reader, _valueType, options)!;
                //
                // // Add to dictionary.
                // dictionary.Add(key, value);
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options)
        {
            // JsonSerializer.Serialize(writer, value, options);
        }
    }
}
