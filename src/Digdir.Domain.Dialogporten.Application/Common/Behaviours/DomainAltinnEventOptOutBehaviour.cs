using Digdir.Domain.Dialogporten.Domain.Common;
using Digdir.Domain.Dialogporten.Domain.Common.DomainEvents;
using MediatR;

namespace Digdir.Domain.Dialogporten.Application.Common.Behaviours;

internal sealed class DomainAltinnEventOptOutBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IAltinnEventDisabler
{
    private readonly IDomainEventContext _domainEventContext;

    public DomainAltinnEventOptOutBehaviour(IDomainEventContext domainEventContext)
    {
        _domainEventContext = domainEventContext;
    }

    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request.DisableAltinnEvents)
        {
            _domainEventContext.AddMetadata(Constants.DisableAltinnEvents, "true");
        }

        return next();
    }
}

public interface IAltinnEventDisabler
{
    bool DisableAltinnEvents { get; }
}
