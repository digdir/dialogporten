using Digdir.Domain.Dialogporten.Domain;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Digdir.Domain.Dialogporten.Infrastructure.Common.Extensions;

internal static class JsonSerializerExtensions
{
    private static readonly Lazy<JsonPolymorphismOptions> _polymorphismOptions = new(() =>
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

    public static JsonSerializerOptions DomainEventPolymorphismOptions = new()
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers =
                {
                    static typeInfo =>
                    {
                        if (typeInfo.Type != typeof(IDomainEvent))
                        {
                            return;
                        }

                        typeInfo.PolymorphismOptions = _polymorphismOptions.Value;
                    },
                }
        }
    };
}