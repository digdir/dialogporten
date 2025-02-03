namespace Digdir.Domain.Dialogporten.Domain;

public static class MediaTypes
{
    public const string EmbeddablePrefix = "application/vnd.dialogporten.frontchannelembed";
    public const string EmbeddableMarkdown = $"{EmbeddablePrefix}-url;type=text/markdown";
    public const string LegacyEmbeddableHtml = $"{EmbeddablePrefix}-url;type=text/html";

    public const string LegacyHtml = "text/html";
    public const string Markdown = "text/markdown";
    public const string PlainText = "text/plain";
}
