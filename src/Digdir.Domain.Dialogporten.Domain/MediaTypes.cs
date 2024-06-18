
namespace Digdir.Domain.Dialogporten.Domain;

public static class MediaTypes
{
    public static string EmbeddableMarkdown => $"{EmbeddablePrefix}+json;type=markdown";
    public static string Markdown => "text/markdown";
    public static string PlainText => "text/plain";
    public static string Html => "text/html";
    public static string EmbeddablePrefix => "application/vnd.dialogporten.frontchannelembed";
}
