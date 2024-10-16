using Digdir.Domain.Dialogporten.Domain.Common.EventPublisher;

namespace Digdir.Domain.Dialogporten.Domain;

public static class MediaTypes
{
    public const string EmbeddablePrefix = "application/vnd.dialogporten.frontchannelembed";
    public const string EmbeddableMarkdown = $"{EmbeddablePrefix}+json;type=markdown";
    public const string LegacyEmbeddableHtml = $"{EmbeddablePrefix}+json;type=html";

    public const string LegacyHtml = "text/html";
    public const string Markdown = "text/markdown";
    public const string PlainText = "text/plain";
}

public static class DomainExtensions
{
    public static IEnumerable<Type> GetDomainEventTypes()
        => DomainAssemblyMarker.Assembly
            .GetTypes()
            .Where(x => !x.IsAbstract && !x.IsInterface && !x.IsGenericType)
            .Where(x => x.IsAssignableTo(typeof(IDomainEvent)));
}
