using AutoMapper;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Content;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Elements;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Search;

internal sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DialogEntity, SearchDialogDto>()
            .ForMember(dest => dest.LatestActivity, opt => opt.MapFrom(src => src.Activities
                .Where(activity => activity.TypeId != DialogActivityType.Values.Forwarded)
                .OrderByDescending(activity => activity.CreatedAt).ThenByDescending(activity => activity.Id)
                .FirstOrDefault()
            ))
            .ForMember(dest => dest.SeenSinceLastUpdate, opt => opt.MapFrom(src => src.SeenLog
                .Where(x => x.CreatedAt >= x.Dialog.UpdatedAt)
                .OrderByDescending(x => x.CreatedAt)
            ))
            .ForMember(dest => dest.GuiAttachmentCount, opt => opt.MapFrom(src => src.Elements
                .Count(x => x.Urls
                    .Any(url => url.ConsumerTypeId == DialogElementUrlConsumerType.Values.Gui))))
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content.Where(x => x.Type.OutputInList)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.StatusId));

        CreateMap<DialogContent, SearchDialogContentDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.TypeId));

        CreateMap<DialogSeenLog, SearchDialogDialogSeenLogDto>()
            .ForMember(dest => dest.SeenAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.EndUserIdHash, opt => opt.MapFrom(src => src.EndUserId));

        CreateMap<DialogActivity, SearchDialogDialogActivityDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.TypeId));
    }
}
