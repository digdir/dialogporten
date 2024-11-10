using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ResourceRegistry.Commands.Synchronize;

internal sealed class SynchronizeResourcePolicyInformationCommandValidator : AbstractValidator<SynchronizeResourcePolicyInformationCommand>
{
    public SynchronizeResourcePolicyInformationCommandValidator()
    {
        RuleFor(x => x.NumberOfConcurrentRequests).InclusiveBetween(1, 50);
    }
}