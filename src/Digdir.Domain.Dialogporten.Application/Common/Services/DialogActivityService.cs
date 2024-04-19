using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Localizations;

namespace Digdir.Domain.Dialogporten.Application.Common.Services;

public interface IDialogActivityService
{
    Task EnsurePerformedByIsSetForActivities(IEnumerable<DialogActivity> activities, CancellationToken cancellationToken);
}

public class DialogActivityService : IDialogActivityService
{
    private readonly IUserOrganizationRegistry _userOrganizationRegistry;

    public DialogActivityService(
        IUserOrganizationRegistry userOrganizationRegistry
    )
    {
        _userOrganizationRegistry = userOrganizationRegistry;
    }

    public async Task EnsurePerformedByIsSetForActivities(IEnumerable<DialogActivity> activities, CancellationToken cancellationToken)
    {
        // TODO: if organization cannot be found we need to handle this. Put on a queue to be retried later(?) https://github.com/digdir/dialogporten/issues/639
        foreach (var activity in activities)
        {
            var organizationLongNames = await _userOrganizationRegistry.GetCurrentUserOrgLongNames(cancellationToken);
            activity.PerformedBy ??= new DialogActivityPerformedBy
            {
                Localizations = organizationLongNames?.Select(x => new Localization { Value = x.LongName, CultureCode = x.Language }).ToList() ?? new List<Localization>(),
            };
        }
    }
}

