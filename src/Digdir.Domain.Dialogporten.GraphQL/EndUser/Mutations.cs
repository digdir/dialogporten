using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogLabels.Commands.Set;
using MediatR;

namespace Digdir.Domain.Dialogporten.GraphQL.EndUser;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<SetSystemLabelInput, SetDialogSystemLabelCommand>()
            .ForMember(dest => dest.Label, opt => opt.MapFrom(src => src.Label));
    }
}

public sealed class Mutations
{
    public async Task<SetSystemLabelPayload> SetSystemLabel(
        [Service] ISender mediator,
        [Service] IMapper mapper,
        SetSystemLabelInput input)
    {
        var command = mapper.Map<SetDialogSystemLabelCommand>(input);
        var result = await mediator.Send(command);

        return result.Match(
            success => new SetSystemLabelPayload { Success = true },
            entityNotFound => new SetSystemLabelPayload { Errors = [new SetSystemLabelEntityNotFound { Message = entityNotFound.Message }] },
            forbidden => new SetSystemLabelPayload { Errors = [new SetSystemLabelForbidden { Message = "forbidden.Reasons" }] },
            entityDeleted => new SetSystemLabelPayload { Errors = [new SetSystemLabelEntityDeleted { Message = entityDeleted.Message }] },
            domainError => new SetSystemLabelPayload { Errors = [new SetSystemLabelDomainError { Message = "domain.Errors" }] },
            concurrencyError => new SetSystemLabelPayload { Errors = [new SetSystemLabelConcurrencyError { Message = "concurry" }] });
    }
}

public sealed class SetSystemLabelPayload
{
    public bool Success { get; set; }
    public List<ISetSystemLabelError> Errors { get; set; } = [];
}

public sealed class SetSystemLabelInput
{
    public Guid DialogId { get; set; }
    public SystemLabel Label { get; set; }
}

public enum SystemLabel
{
    Default = 1,
    Trash = 2,
    Archive = 3
}

[InterfaceType("SetSystemLabelError")]
public interface ISetSystemLabelError
{
    public string Message { get; set; }
}

public sealed class SetSystemLabelEntityNotFound : ISetSystemLabelError
{
    public string Message { get; set; } = null!;
}

public sealed class SetSystemLabelForbidden : ISetSystemLabelError
{
    public string Message { get; set; } = null!;
}

public sealed class SetSystemLabelDomainError : ISetSystemLabelError
{
    public string Message { get; set; } = null!;
}

public sealed class SetSystemLabelConcurrencyError : ISetSystemLabelError
{
    public string Message { get; set; } = null!;
}

public sealed class SetSystemLabelEntityDeleted : ISetSystemLabelError
{
    public string Message { get; set; } = null!;
}
