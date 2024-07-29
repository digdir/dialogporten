using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Domain;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.Content;

public class ContentValueDto
{
    public List<LocalizationDto> Value { get; set; } = [];
    public string MediaType { get; set; } = MediaTypes.PlainText;
}
