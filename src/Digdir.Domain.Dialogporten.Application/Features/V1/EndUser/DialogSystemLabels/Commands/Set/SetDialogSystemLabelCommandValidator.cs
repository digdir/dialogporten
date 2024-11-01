using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogSystemLabels.Commands.Set;

public sealed class SetDialogSystemLabelCommandValidator : AbstractValidator<SystemLabelCommand>
{
    public SetDialogSystemLabelCommandValidator()
    {
        RuleFor(x => x.Label)
            .NotNull();
    }
}
