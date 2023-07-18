using AutoMapper;

namespace Digdir.Domain.Dialogporten.Application.Common;

internal sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DateTimeOffset, DateTime>().ConstructUsing(dateTimeOffset => dateTimeOffset.UtcDateTime);
    }
}