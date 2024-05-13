using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Localizations;

namespace Digdir.Domain.Dialogporten.Application.Common.Services;

public interface IDialogActivityService
{
    Task EnsurePerformedByIsSetForActivities(IEnumerable<DialogActivity> activities, CancellationToken cancellationToken);
}

public class DialogActivityService : IDialogActivityService
{
    private readonly IUserRegistry _userRegistry;

    public DialogActivityService(IUserRegistry userRegistry)
    {
        _userRegistry = userRegistry;
    }

    public async Task EnsurePerformedByIsSetForActivities(IEnumerable<DialogActivity> activities, CancellationToken cancellationToken)
    {
        // TODO: if organization cannot be found we need to handle this. Put on a queue to be retried later(?) https://github.com/digdir/dialogporten/issues/639
        foreach (var activity in activities)
        {
            var userInformation = await _userRegistry.GetCurrentUserInformation(cancellationToken);

            activity.PerformedBy ??= new DialogActivityPerformedBy
            {
                // TODO: Ask Bjørn about returned null values from name lookups (remove ! claims below)
                Localizations = userInformation.LocalizedNames
                    // .Where(x => x.Name is not null && x.LanguageCode is not null) // TOTO: REMOVE THIS
                    .Select(x => new Localization { Value = x.Name!, CultureCode = x.LanguageCode! }).ToList(),
            };
        }
    }
}
