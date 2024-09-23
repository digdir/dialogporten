using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.Authorization;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogDisplayState.Commands;

[GenerateOneOf]
public sealed partial class SetDisplayStateResult : OneOfBase<Success, EntityNotFound, BadRequest, ValidationError, Forbidden, DomainError, ConcurrencyError>;

public sealed class SetDisplayStateCommand : IRequest<SetDisplayStateResult>
{

}

internal sealed class SetDisplayStateCommandHandler(
    IDialogDbContext dialogDbContext,
    IMapper mapper,
    IUnitOfWork unitOfWork,
    IDomainContext domainContext,
    IUserResourceRegistry userResourceRegistry,
    IServiceResourceAuthorizer serviceResourceAuthorizer)
    : IRequestHandler<SetDisplayStateCommand, SetDisplayStateResult>
{
    private readonly IDialogDbContext _dialogDbContext = dialogDbContext ?? throw new ArgumentNullException(nameof(dialogDbContext));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IDomainContext _domainContext = domainContext ?? throw new ArgumentNullException(nameof(domainContext));
    private readonly IUserResourceRegistry _userResourceRegistry = userResourceRegistry ?? throw new ArgumentNullException(nameof(userResourceRegistry));
    private readonly IServiceResourceAuthorizer _serviceResourceAuthorizer = serviceResourceAuthorizer ?? throw new ArgumentNullException(nameof(serviceResourceAuthorizer));

    public Task<SetDisplayStateResult> Handle(SetDisplayStateCommand request, CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
