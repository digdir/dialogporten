using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using FluentValidation;
using HtmlAgilityPack;

namespace Digdir.Domain.Dialogporten.Application.Common.Extensions.FluentValidation;

internal static class FluentValidationLocalizationDtoExtensions
{
    private static readonly string[] AllowedTags = ["p", "a", "br", "em", "strong", "ul", "ol", "li"];
    private static readonly string ContainsValidHtmlError =
        $"{{PropertyName}} contains unsupported html. The following tags are supported: " +
        $"[{string.Join(",", AllowedTags.Select(x => '<' + x + '>'))}]. Tag attributes " +
        $"are not supported except for on '<a>' which must contain a 'href' starting " +
        $"with 'https://'.";

    public static IRuleBuilderOptions<T, LocalizationDto> ContainsValidHtml<T>(this IRuleBuilder<T, LocalizationDto> ruleBuilder)
    {
        return ruleBuilder
            .Must(x => x.Value is null || x.Value.HtmlAgilityPackCheck())
            .WithMessage(ContainsValidHtmlError);
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
        const string anchorTag = "a";
        return node.Name == anchorTag;
    }

    private static bool IsValidAnchorTag(this HtmlNode node)
    {
        const string https = "https://";
        const string href = "href";
        return node.Attributes.Count == 1 &&
            node.Attributes[href] is not null &&
            node.Attributes[href].Value.StartsWith(https, StringComparison.InvariantCultureIgnoreCase);
    }
}
