using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ResourceRegistry.Commands.SyncPolicy;

internal sealed class SyncPolicyCommandValidator : AbstractValidator<SyncPolicyCommand>
{
    public SyncPolicyCommandValidator()
    {
        RuleFor(x => x.NumberOfConcurrentRequests).InclusiveBetween(1, 50);
    }
}
