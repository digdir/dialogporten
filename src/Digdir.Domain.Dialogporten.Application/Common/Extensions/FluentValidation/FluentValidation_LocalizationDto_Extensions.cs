using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using FluentValidation;
using HtmlAgilityPack;

namespace Digdir.Domain.Dialogporten.Application.Common.Extensions.FluentValidation;

internal static class FluentValidation_LocalizationDto_Extensions
{
    private static readonly string[] AllowedTags = new[] { "p", "a", "br", "em", "strong", "ul", "ol", "li" };
    private static readonly string ContainsValidHttpError = 
        $"{{PropertyName}} contains unsupported html. The following tags are supported: " +
        $"[{string.Join(",", AllowedTags.Select(x => '<' + x + '>'))}]. Tag atributes " +
        $"are not supported except for on '<a>' which must contain a 'href' starting " +
        $"with 'https://'.";

    public static IRuleBuilderOptions<T, LocalizationDto> ContainsValidHttp<T>(this IRuleBuilder<T, LocalizationDto> ruleBuilder)
    {
        return ruleBuilder
            .Must(x => x.Value is null || x.Value.HtmlAgilityPackCheck())
            .WithMessage(ContainsValidHttpError);
    }

    private static bool HtmlAgilityPackCheck(this string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var nodes = doc.DocumentNode.DescendantsAndSelf();
        foreach (var node in nodes)
        {
            if (node.NodeType != HtmlNodeType.Element) continue;

            if (!AllowedTags.Contains(node.Name))
            {
                return false;
            }
            // If the node is a hyperlink, it should only have an href attribute
            // and it should start with 'https://'
            if (node.IsAnchorTag())
            {
                if (!node.IsValidAnchorTag())
                {
                    return false;
                }

                continue;
            }

            if (node.Attributes.Count > 0)
            {
                return false;
            }
        }
        return true;
    }

    private static bool IsAnchorTag(this HtmlNode node)
    {
        const string AnchorTag = "a";
        return node.Name == AnchorTag;
    }

    private static bool IsValidAnchorTag(this HtmlNode node)
    {
        const string Https = "https://";
        const string Href = "href";
        return node.Attributes.Count == 1 &&
            node.Attributes[Href] is not null &&
            node.Attributes[Href].Value.StartsWith(Https);
    }
}