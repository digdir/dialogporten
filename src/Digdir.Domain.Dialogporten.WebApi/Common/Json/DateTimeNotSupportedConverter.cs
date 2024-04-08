using System.Text.Json;
using System.Text.Json.Serialization;

namespace Digdir.Domain.Dialogporten.WebApi.Common.Json;

internal sealed class DateTimeNotSupportedConverter : JsonConverter<DateTime>
{
    private const string ErrorMessage =
        "DateTime is not supported, use DateTimeOffset instead. " +
        "This is a service error, not a consumer error. Please " +
        "contact the support team if you're experiencing this " +
        "error as a consumer.";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException(ErrorMessage);

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => throw new NotSupportedException(ErrorMessage);
}
