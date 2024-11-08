using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ResourceRegistry.Commands.Synchronize;

internal sealed class SynchronizeResourcePolicyMetadataCommandValidator : AbstractValidator<SynchronizeResourcePolicyMetadataCommand>
{
    public SynchronizeResourcePolicyMetadataCommandValidator()
    {
        RuleFor(x => x.NumberOfConcurrentRequests).InclusiveBetween(1, 50);
    }
}
