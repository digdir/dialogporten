using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Digdir.Library.Entity.Abstractions.Features.EventPublisher;

namespace Digdir.Domain.Dialogporten.Infrastructure.Common.Serialization;

internal static class SerializerOptions
{
    private static readonly Lazy<JsonPolymorphismOptions> PolymorphismOptions = new(() =>
    {
        var options = new JsonPolymorphismOptions();
        var domainEventType = typeof(IDomainEvent);
        var derivedTypes = domainEventType
            .Assembly
            .GetExportedTypes()
            .Where(x => x.IsAssignableTo(domainEventType) && !x.IsAbstract && !x.IsInterface)
            .Select(x => new JsonDerivedType(x, x.Name));

        foreach (var derivedType in derivedTypes)
        {
            options.DerivedTypes.Add(derivedType);
        }

        return options;
    });

    public static readonly JsonSerializerOptions CloudEventSerializerOptions = new()
    {
        PropertyNamingPolicy = new LowerCaseNamingPolicy(),
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };
    public static JsonSerializerOptions DomainEventPolymorphismOptions = new()
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers = { DomainEventModifier }
        }
    };

    private static void DomainEventModifier(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Type != typeof(IDomainEvent))
        {
            return;
        }

        typeInfo.PolymorphismOptions = PolymorphismOptions.Value;
    }
}

internal class LowerCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name) =>
        name.ToLowerInvariant();
}
