using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Queries.EndUser.Get;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Commands.Update;

internal sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        // In
        CreateMap<UpdateDialogDto, DialogEntity>()
            .IgnoreComplexDestinationProperties()
			.ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => src.Status));

        CreateMap<UpdateDialogDialogApiActionDto, DialogApiAction>()
            .IgnoreComplexDestinationProperties()
            .ForMember(x => x.Id, opt => opt.Ignore());
        CreateMap<UpdateDialogDialogApiActionEndpointDto, DialogApiActionEndpoint>()
            .IgnoreComplexDestinationProperties()
            .ForMember(x => x.Id, opt => opt.Ignore())
			.ForMember(dest => dest.HttpMethod, opt => opt.Ignore())
            .ForMember(dest => dest.HttpMethodId, opt => opt.MapFrom(src => src.HttpMethod));

        CreateMap<UpdateDialogDialogGuiActionDto, DialogGuiAction>()
            .IgnoreComplexDestinationProperties()
            .ForMember(x => x.Id, opt => opt.Ignore())
			.ForMember(dest => dest.Priority, opt => opt.Ignore())
            .ForMember(dest => dest.PriorityId, opt => opt.MapFrom(src => src.Priority));

        CreateMap<UpdateDialogDialogElementDto, DialogElement>()
            .IgnoreComplexDestinationProperties();
        CreateMap<UpdateDialogDialogElementUrlDto, DialogElementUrl>()
            .IgnoreComplexDestinationProperties()
            .ForMember(x => x.Id, opt => opt.Ignore())
			.ForMember(dest => dest.ConsumerType, opt => opt.Ignore())
            .ForMember(dest => dest.ConsumerTypeId, opt => opt.MapFrom(src => src.ConsumerType));

        // Since this is append only, we don't need to merge with existing
        // activity records and thus can map complex properties
        CreateMap<UpdateDialogDialogActivityDto, DialogActivity>()
			.ForMember(dest => dest.Type, opt => opt.Ignore())
            .ForMember(dest => dest.TypeId, opt => opt.MapFrom(src => src.Type));

        // To support json patch
        CreateMap<GetDialogDto, UpdateDialogDto>();
        CreateMap<GetDialogDialogActivityDto, UpdateDialogDialogActivityDto>();
        CreateMap<GetDialogDialogApiActionDto, UpdateDialogDialogApiActionDto>();
        CreateMap<GetDialogDialogApiActionEndpointDto, UpdateDialogDialogApiActionEndpointDto>();
        CreateMap<GetDialogDialogGuiActionDto, UpdateDialogDialogGuiActionDto>();
        CreateMap<GetDialogDialogElementDto, UpdateDialogDialogElementDto>();
        CreateMap<GetDialogDialogElementUrlDto, UpdateDialogDialogElementUrlDto>();
        CreateMap<DateTimeOffset, DateTime>().ConstructUsing(dateTimeOffset => dateTimeOffset.UtcDateTime);
    }
}
