using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ResourceRegistry.Commands.SyncSubjectMap;

internal sealed class SyncSubjectMapCommandValidator : AbstractValidator<SyncSubjectMapCommand>
{
    public SyncSubjectMapCommandValidator()
    {
        RuleFor(x => x.BatchSize).InclusiveBetween(1, 1000);
    }
}
