using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Domain;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.Content;

public class ContentValueDto
{
    /// <summary>
    /// A list of localizations for the content
    /// </summary>
    public List<LocalizationDto> Value { get; set; } = [];

    /// <summary>
    /// Media type of the content (plaintext, markdown, html). Can also indicate that the content is embeddable.
    /// </summary>
    /// <example>
    /// text/plain
    /// text/markdown
    /// application/vnd.dialogporten.frontchannelembed
    /// </example>
    public string MediaType { get; set; } = MediaTypes.PlainText;
}
