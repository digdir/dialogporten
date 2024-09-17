using System.Globalization;
using System.Text;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.SubjectResources;
using Microsoft.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Application.Common.Extensions;

public static class DbSetExtensions
{
    public static IQueryable<DialogEntity> PrefilterAuthorizedDialogs(this DbSet<DialogEntity> dialogs,
        DialogSearchAuthorizationResult authorizedResources)
    {
        var parameters = new List<object>();

        // lang=sql
        var sb = new StringBuilder()
            .AppendLine(CultureInfo.InvariantCulture, $"""
                                                       SELECT *
                                                       FROM "Dialog"
                                                       WHERE "Id" = ANY(@p{parameters.Count})
                                                       """);
        parameters.Add(authorizedResources.DialogIds);

        foreach (var (party, resources) in authorizedResources.ResourcesByParties)
        {
            // lang=sql
            sb.AppendLine(CultureInfo.InvariantCulture, $"""
                                                         OR (
                                                            "{nameof(DialogEntity.Party)}" = @p{parameters.Count} 
                                                            AND "{nameof(DialogEntity.ServiceResource)}" = ANY(@p{parameters.Count + 1})
                                                         )
                                                         """);
            parameters.Add(party);
            parameters.Add(resources);
        }

        foreach (var (party, subjects) in authorizedResources.SubjectsByParties)
        {
            // lang=sql
            sb.AppendLine(CultureInfo.InvariantCulture, $"""
                                                         OR (
                                                            "{nameof(DialogEntity.Party)}" = @p{parameters.Count} 
                                                            AND "{nameof(DialogEntity.ServiceResource)}" = ANY(
                                                                SELECT "{nameof(SubjectResource.Resource)}" 
                                                                FROM "{nameof(SubjectResource)}" 
                                                                WHERE "{nameof(SubjectResource.Subject)}" = ANY(@p{parameters.Count + 1})
                                                            )
                                                         )
                                                         """);
            parameters.Add(party);
            parameters.Add(subjects);
        }

        return dialogs.FromSqlRaw(sb.ToString(), parameters.ToArray());
    }
}
