using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ResourceRegistry.Commands.Synchronize;

internal sealed class SynchronizeSubjectResourceMappingsCommandValidator : AbstractValidator<SynchronizeSubjectResourceMappingsCommand>
{
    public SynchronizeSubjectResourceMappingsCommandValidator()
    {
        RuleFor(x => x.BatchSize).InclusiveBetween(1, 1000);
    }
}
