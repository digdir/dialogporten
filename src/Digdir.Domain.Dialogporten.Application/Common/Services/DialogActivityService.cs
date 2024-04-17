namespace Digdir.Domain.Dialogporten.Application.Common.Services;

public class DialogActivityService
{
    public DialogActivityService(
        IUserOrganizationRegistry userOrganizationRegistry
    )
    {

    }

    public static void EnsurePerformedByIsSetForActivities(
        IEnumerable<DialogActivity> activities)
    {
        foreach (var activity in activities)
        {
            activity.PerformedBy ??= new DialogActivityPerformedBy
            {
                Localizations = organizationLongNames?.Select(x => new Localization { Value = x.LongName, CultureCode = x.Language }).ToList() ?? new List<Localization>(),
            };
        }
    }
}

