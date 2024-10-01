using Digdir.Domain.Dialogporten.Domain.DialogEndUserContexts.Entities;
using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogLabels.Commands.Set;

public sealed class SetDialogLabelCommandValidator : AbstractValidator<SetDialogLabelCommand>
{
    public SetDialogLabelCommandValidator()
    {
        RuleFor(x => x.Label)
            .NotNull();
        RuleFor(x => x.Label)
            .Must(x => x.StartsWith(SystemLabel.PrefixWithSeparator, StringComparison.InvariantCulture))
            .When(x => x.Label is not null)
            .WithMessage($"'{{PropertyName}}' must start with '{SystemLabel.PrefixWithSeparator}'.");
        RuleFor(x => x.Label)
            .Must(x => Enum.TryParse(x.Split(":")[1], true, out SystemLabel.Values _))
            .When(x => x.Label is not null && x.Label.Contains(':'))
            .WithMessage("'{PropertyName}' is not a valid value.");
    }
}
