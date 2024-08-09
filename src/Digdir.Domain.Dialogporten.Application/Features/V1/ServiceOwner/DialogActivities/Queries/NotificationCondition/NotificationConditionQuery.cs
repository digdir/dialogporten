using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogActivities.Queries.NotificationCondition;

public sealed class NotificationConditionQuery : IRequest<NotificationConditionResult>
{
    public Guid DialogId { get; set; }
    public NotificationConditionType ConditionType { get; set; }
    public DialogActivityType.Values ActivityType { get; set; }
    public Guid? TransmissionId { get; set; }
}

public enum NotificationConditionType
{
    NotExists = 1,
    Exists = 2
}

[GenerateOneOf]
public partial class NotificationConditionResult : OneOfBase<NotificationConditionDto, ValidationError, EntityNotFound>;

internal sealed class NotificationConditionQueryHandler : IRequestHandler<NotificationConditionQuery, NotificationConditionResult>
{
    private readonly IDialogDbContext _db;

    public NotificationConditionQueryHandler(IDialogDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<NotificationConditionResult> Handle(NotificationConditionQuery request, CancellationToken cancellationToken)
    {
        var dialog = await _db.Dialogs
            .Include(x => x.Activities)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == request.DialogId,
                cancellationToken: cancellationToken);

        if (dialog is null)
        {
            return new EntityNotFound<DialogEntity>(request.DialogId);
        }

        var conditionMet = CheckDialogActivitiesCondition(dialog.Activities, request.ConditionType, request.ActivityType, request.TransmissionId);

        return new NotificationConditionDto { SendNotification = conditionMet };
    }

    private static bool CheckDialogActivitiesCondition(
        List<DialogActivity> activities,
        NotificationConditionType conditionType,
        DialogActivityType.Values activityType,
        Guid? transmissionId) =>
        activities.Where(
                x => x.TypeId == activityType
                     && (transmissionId is null || x.TransmissionId == transmissionId)).ToList()
            .Count == 0
            ? conditionType == NotificationConditionType.NotExists
            : conditionType == NotificationConditionType.Exists;
}
