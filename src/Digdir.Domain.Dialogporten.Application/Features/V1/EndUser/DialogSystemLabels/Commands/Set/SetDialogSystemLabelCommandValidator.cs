using Digdir.Domain.Dialogporten.Domain.DialogEndUserContexts.Entities;
using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogSystemLabels.Commands.Set;

public sealed class SetDialogSystemLabelCommandValidator : AbstractValidator<SetDialogSystemLabelCommand>
{
    public SetDialogSystemLabelCommandValidator()
    {
        RuleFor(x => x.Label)
            .NotNull();
    }
}
