using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogActivities.Queries.NotificationCondition;

internal sealed class NotificationConditionQueryValidator : AbstractValidator<NotificationConditionQuery>
{
    public NotificationConditionQueryValidator()
    {
        RuleFor(x => x.DialogId)
            .NotEqual(default(Guid));

        RuleFor(x => x.ConditionType)
            .NotNull()
            .IsInEnum();

        RuleFor(x => x.ActivityType)
            .NotNull()
            .IsInEnum();

        RuleFor(x => x.TransmissionId)
            .NotNull()
            .NotEqual(default(Guid))
            .When(x => x.ActivityType == DialogDialogActivityType.Values.TransmissionOpened)
            .WithMessage($"{{PropertyName}} must not be empty when ${nameof(NotificationConditionQuery.ActivityType)} is ${nameof(DialogDialogActivityType.Values.TransmissionOpened)}");

        RuleFor(x => x.TransmissionId)
            .Null()
            .When(x => x.ActivityType == DialogDialogActivityType.Values.DialogOpened)
            .WithMessage($"{{PropertyName}} must be empty when {nameof(NotificationConditionQuery.ActivityType)} is {nameof(DialogDialogActivityType.Values.DialogOpened)}.");
    }
}
