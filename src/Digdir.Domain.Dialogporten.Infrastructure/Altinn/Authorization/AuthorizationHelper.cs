using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;
using Digdir.Domain.Dialogporten.Domain.SubjectResources;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.Authorization;

internal static class AuthorizationHelper
{
    public static async Task CollapseSubjectResources(
        DialogSearchAuthorizationResult dialogSearchAuthorizationResult,
        AuthorizedPartiesResult authorizedParties,
        List<string> constraintResources,
        Func<CancellationToken, Task<List<SubjectResource>>> getAllSubjectResources,
        CancellationToken cancellationToken)
    {
        var authorizedPartiesWithRoles = authorizedParties.AuthorizedParties
            .Where(p => p.AuthorizedRoles.Count != 0)
            .ToList();

        var uniqueSubjects = authorizedPartiesWithRoles
            .SelectMany(p => p.AuthorizedRoles)
            .ToHashSet();

        var subjectResources = (await getAllSubjectResources(cancellationToken))
            .Where(x => uniqueSubjects.Contains(x.Subject) && (constraintResources.Count == 0 || constraintResources.Contains(x.Resource))).ToList();

        var subjectToResources = subjectResources
            .GroupBy(sr => sr.Subject)
            .ToDictionary(g => g.Key, g => g.Select(sr => sr.Resource).ToHashSet());

        foreach (var partyEntry in authorizedPartiesWithRoles)
        {
            if (!dialogSearchAuthorizationResult.ResourcesByParties.TryGetValue(partyEntry.Party, out var resourceList))
            {
                resourceList = new HashSet<string>();
                dialogSearchAuthorizationResult.ResourcesByParties[partyEntry.Party] = resourceList;
            }

            foreach (var subject in partyEntry.AuthorizedRoles)
            {
                if (subjectToResources.TryGetValue(subject, out var subjectResourceSet))
                {
                    resourceList.UnionWith(subjectResourceSet);
                }
            }

            if (resourceList.Count == 0)
            {
                dialogSearchAuthorizationResult.ResourcesByParties.Remove(partyEntry.Party);
            }
        }
    }
}
