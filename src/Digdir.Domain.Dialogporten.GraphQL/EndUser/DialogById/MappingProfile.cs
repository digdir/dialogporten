using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.GraphQL.EndUser.Common;

namespace Digdir.Domain.Dialogporten.GraphQL.EndUser.DialogById;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<GetDialogDto, Dialog>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

        CreateMap<GetDialogDialogAttachmentDto, Attachment>();
        CreateMap<GetDialogDialogAttachmentUrlDto, AttachmentUrl>()
            .ForMember(dest => dest.ConsumerType, opt => opt.MapFrom(src => src.ConsumerType));

        CreateMap<GetDialogDialogGuiActionDto, GuiAction>()
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority))
            .ForMember(dest => dest.HttpMethod, opt => opt.MapFrom(src => src.HttpMethod));

        CreateMap<GetDialogDialogApiActionDto, ApiAction>();
        CreateMap<GetDialogDialogApiActionEndpointDto, ApiActionEndpoint>()
            .ForMember(dest => dest.HttpMethod, opt => opt.MapFrom(src => src.HttpMethod));

        CreateMap<GetDialogContentDto, Content>();

        CreateMap<GetDialogDialogSeenLogDto, SeenLog>();
    }
}
