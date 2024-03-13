using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Purge;

internal sealed class PurgeDialogCommandValidator : AbstractValidator<PurgeDialogCommand>
{
    public PurgeDialogCommandValidator()
    {
        RuleFor(x => x.DialogId)
            .NotEqual(Guid.Empty)
            .WithMessage($"{{PropertyName}} was either badly formatted or {Guid.Empty}");
    }
}
