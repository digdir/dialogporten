using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Application.Externals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Localizations;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.DialogActivities;

public static class DialogActivityExtensions
{
    public static void EnsurePerformedByIsSetForActivities(
        this IEnumerable<DialogActivity> activities,
        IList<OrganizationLongName> organizationLongNames)
    {
        foreach (var activity in activities)
        {
            activity.PerformedBy ??= new DialogActivityPerformedBy
            {
                ActivityId = activity.Id,
                Id = Guid.NewGuid(),
                Localizations = organizationLongNames?.Select(x => new Localization { Value = x.LongName, CultureCode = x.Language }).ToList() ?? new List<Localization>(),
            };
        }
    }
}