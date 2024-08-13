namespace Digdir.Domain.Dialogporten.Domain;

public static class MediaTypes
{
    public const string EmbeddablePrefix = "application/vnd.dialogporten.frontchannelembed";
    public const string EmbeddableMarkdown = $"{EmbeddablePrefix}+json;type=markdown";

    public const string Markdown = "text/markdown";
    public const string PlainText = "text/plain";
    public const string Html = "text/html";
}
