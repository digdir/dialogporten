using AutoMapper;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogSeenLogs.Queries.Search;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DialogSeenLog, SearchDialogSeenLogDto>();
    }
}
