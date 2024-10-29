using System.Text;
using Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations;
using Digdir.Domain.Dialogporten.WebApi.Common.Swagger;
using NJsonSchema.Generation;

namespace Digdir.Domain.Dialogporten.WebApi.Common.Json;

/// <summary>
/// Use the SchemaNameGenerator from FastEndpoints as a base. Code copied from
/// https://github.com/FastEndpoints/FastEndpoints/blob/05c93fa58c013087f33176b07647e1fe07f38431/Src/Swagger/SchemaNameGenerator.cs, which is sealed :/
///
/// For this base, remove the "Dto" suffix from the generated schema names, and add a "SO" suffix to the serviceowner specific schemas. This
/// matches the "SO" suffix used on the operationIds for service owners.
///
/// </summary>
internal sealed class SuffixedSchemaNameGenerator : ISchemaNameGenerator
{
    public string Generate(Type type) => TypeNameConverter.ToShortNameStrict(type);
}
