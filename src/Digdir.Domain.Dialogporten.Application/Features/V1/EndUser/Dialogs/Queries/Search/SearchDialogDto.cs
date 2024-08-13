using System.Text.Json.Serialization;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Content;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Contents;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Search;

public sealed class SearchDialogDto : SearchDialogDtoBase
{
    [JsonPropertyOrder(100)] // ILU MAGNUS
    public SearchDialogContentDto Content { get; set; } = null!;
}

public sealed class SearchDialogContentDto
{
    public ContentValueDto Title { get; set; } = null!;
    public ContentValueDto Summary { get; set; } = null!;
    public ContentValueDto? SenderName { get; set; }
    public ContentValueDto? ExtendedStatus { get; set; }
}

/// <summary>
/// TOOD: Discuss this with the team later. It works for now
/// This class is used in order to keep using ProjectTo and existing PaginationList code.
/// We first map to this using ProjectTo, then map to the new DialogContent structure
/// in the SearchDialog handlers, after EF core is done loading the data.
/// Then we create a new PaginatedList with the outwards facing dto
/// </summary>
public sealed class IntermediateSearchDialogDto : SearchDialogDtoBase
{
    public List<DialogContent> Content { get; set; } = [];
}
