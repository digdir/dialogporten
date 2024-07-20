using System.Text;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Microsoft.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Application.Common.Extensions;

public static class DbSetExtensions
{
    public static IQueryable<DialogEntity> PrefilterAuthorizedDialogs(this DbSet<DialogEntity> dialogs,
        Func<Task<DialogSearchAuthorizationResult?>> authorizedResourcesProvider)
    {
        var authorizedResources = authorizedResourcesProvider().GetAwaiter().GetResult();
        return authorizedResources is null ? dialogs : dialogs.PrefilterAuthorizedDialogs(authorizedResources);
    }

    public static IQueryable<DialogEntity> PrefilterAuthorizedDialogs(this DbSet<DialogEntity> dialogs, DialogSearchAuthorizationResult authorizedResources)
    {
        var parameters = new List<object>();
        var sb = new StringBuilder().Append("SELECT * FROM \"Dialog\" WHERE 1=1");

        foreach (var item in authorizedResources.ResourcesByParties)
        {
            sb.Append(" OR (\"Party\" = @p")
                .Append(parameters.Count)
                .Append(" AND \"ServiceResource\" = ANY(@p")
                .Append(parameters.Count + 1).Append("))");
            parameters.Add(item.Key);
            parameters.Add(item.Value);
        }

        foreach (var item in authorizedResources.RolesByParties)
        {
            sb.Append(" OR (\"Party\" = @p")
                .Append(parameters.Count)
                .Append(" AND \"ServiceResource\" = ANY(SELECT r.\"Resource\" FROM \"RoleResource\" r WHERE r.\"Role\" = ANY(@p")
                .Append(parameters.Count + 1).Append(")))");
            parameters.Add(item.Key);
            parameters.Add(item.Value);
        }

        sb.Append(" OR \"Id\" = ANY(@p")
            .Append(parameters.Count)
            .Append(')');
        parameters.Add(authorizedResources.DialogIds);

        return dialogs.FromSqlRaw(sb.ToString(), parameters.ToArray());
    }
}
