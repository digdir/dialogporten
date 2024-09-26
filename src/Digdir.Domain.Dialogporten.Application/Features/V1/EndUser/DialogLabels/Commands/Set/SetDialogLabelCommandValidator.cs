using Digdir.Domain.Dialogporten.Domain.Common;
using Digdir.Domain.Dialogporten.Domain.DialogEndUserContexts.Entities;
using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogLabels.Commands.Set;

public sealed class SetDialogLabelCommandValidator : AbstractValidator<SetDialogLabelCommand>
{
    public SetDialogLabelCommandValidator()
    {
        RuleFor(x => x.Label).NotNull().Must(x =>
                x.StartsWith(Constants.SystemLabelPrefix, StringComparison.InvariantCulture))
            .WithMessage($"'{{PropertyName}}' must start with '{Constants.SystemLabelPrefix}'.");
        RuleFor(x => x.Label)
            .NotNull()
            .Must(x => Enum.TryParse(x.Split(":")[1], true, out SystemLabel.Values _))
            .WithMessage($"'{{PropertyName}}' is not a valid value.")
            .When(x => x.Label.Contains(':'));
    }
}
