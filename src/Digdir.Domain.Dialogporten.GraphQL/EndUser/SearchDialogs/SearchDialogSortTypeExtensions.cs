using System.Globalization;
using System.Text;
using Digdir.Domain.Dialogporten.Application.Common.Pagination.Order;
using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Search;

namespace Digdir.Domain.Dialogporten.GraphQL.EndUser.SearchDialogs;

internal static class SearchDialogSortTypeExtensions
{
    public static List<SearchDialogSortType> ToSearchDialogSortTypeList(
        this string orderBy)
    {
        List<SearchDialogSortType> searchDialogSortTypes = [];

        var orderByParts = orderBy
            .ToLower(CultureInfo.InvariantCulture)
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Where(part => !part.Equals("id_desc", StringComparison.OrdinalIgnoreCase) &&
                           !part.Equals("id_asc", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var orderByPart in orderByParts)
        {
            var parts = orderByPart.Split('_');
            if (parts.Length != 2)
            {
                continue;
            }

            var sortDirection = parts[1] switch
            {
                "asc" => SortDirection.Asc,
                "desc" => SortDirection.Desc,
                _ => throw new InvalidOperationException("Invalid sort direction")
            };

            searchDialogSortTypes.Add(parts[0] switch
            {
                "createdat" => new SearchDialogSortType { CreatedAt = sortDirection },
                "updatedat" => new SearchDialogSortType { UpdatedAt = sortDirection },
                "dueat" => new SearchDialogSortType { DueAt = sortDirection },
                _ => throw new InvalidOperationException("Invalid sort field")
            });
        }

        return searchDialogSortTypes;
    }

    public static bool TryToOrderSet(this List<SearchDialogSortType> searchDialogSortTypes,
        out OrderSet<SearchDialogQueryOrderDefinition, IntermediateDialogDto>? orderSet)
    {
        var stringBuilder = new StringBuilder();
        foreach (var orderBy in searchDialogSortTypes)
        {
            if (orderBy.CreatedAt != null)
            {
                stringBuilder.Append(CultureInfo.InvariantCulture, $"createdAt_{orderBy.CreatedAt},");
                continue;
            }

            if (orderBy.UpdatedAt != null)
            {
                stringBuilder.Append(CultureInfo.InvariantCulture, $"updatedAt_{orderBy.UpdatedAt},");
                continue;
            }

            if (orderBy.DueAt != null)
            {
                stringBuilder.Append(CultureInfo.InvariantCulture, $"dueAt_{orderBy.DueAt},");
            }
        }

        if (OrderSet<SearchDialogQueryOrderDefinition, IntermediateDialogDto>.TryParse(stringBuilder.ToString(),
                out var parsedOrderSet))
        {
            orderSet = parsedOrderSet;
            return true;
        }

        orderSet = null;
        return false;
    }
}
