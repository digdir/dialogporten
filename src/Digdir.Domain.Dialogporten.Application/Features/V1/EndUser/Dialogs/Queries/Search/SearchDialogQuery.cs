using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Common.Extensions.Enumerables;
using Digdir.Domain.Dialogporten.Application.Common.Pagination;
using Digdir.Domain.Dialogporten.Application.Common.Pagination.Extensions;
using Digdir.Domain.Dialogporten.Application.Common.Pagination.OrderOption;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Search;

public sealed class SearchDialogQuery : SortablePaginationParameter<SearchDialogQueryOrderDefinition, SearchDialogDto>, IRequest<SearchDialogResult>
{
    private readonly string? _searchCultureCode;

    /// <summary>
    /// Filter by one or more service owner codes
    /// </summary>
    public List<string>? Org { get; init; }

    /// <summary>
    /// Filter by one or more service resources
    /// </summary>
    public List<string>? ServiceResource { get; init; }

    /// <summary>
    /// Filter by one or more owning parties
    /// </summary>
    public List<string>? Party { get; init; }

    /// <summary>
    /// Filter by one or more extended statuses
    /// </summary>
    public List<string>? ExtendedStatus { get; init; }

    /// <summary>
    /// Filter by external reference
    /// </summary>
    public string? ExternalReference { get; init; }

    /// <summary>
    /// Filter by status
    /// </summary>
    public List<DialogStatus.Values>? Status { get; init; }

    /// <summary>
    /// Only return dialogs created after this date
    /// </summary>
    public DateTimeOffset? CreatedAfter { get; init; }

    /// <summary>
    /// Only return dialogs created before this date
    /// </summary>
    public DateTimeOffset? CreatedBefore { get; init; }

    /// <summary>
    /// Only return dialogs updated after this date
    /// </summary>
    public DateTimeOffset? UpdatedAfter { get; init; }

    /// <summary>
    /// Only return dialogs updated before this date
    /// </summary>
    public DateTimeOffset? UpdatedBefore { get; init; }

    /// <summary>
    /// Only return dialogs with due date after this date
    /// </summary>
    public DateTimeOffset? DueAfter { get; init; }

    /// <summary>
    /// Only return dialogs with due date before this date
    /// </summary>
    public DateTimeOffset? DueBefore { get; init; }

    /// <summary>
    /// Search string for free text search. Will attempt to fuzzily match in all free text fields in the aggregate
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    /// Limit free text search to texts with this culture code, e.g. \"nb-NO\". Default: search all culture codes
    /// </summary>
    public string? SearchCultureCode
    {
        get => _searchCultureCode;
        init => _searchCultureCode = Localization.NormalizeCultureCode(value);
    }
}

public sealed class SearchDialogQueryOrderDefinition : IOrderDefinition<SearchDialogDto>
{
    public static IOrderOptions<SearchDialogDto> Configure(IOrderOptionsBuilder<SearchDialogDto> options) =>
        options.AddId(x => x.Id)
            .AddDefault("createdAt", x => x.CreatedAt)
            .AddOption("updatedAt", x => x.UpdatedAt)
            .AddOption("dueAt", x => x.DueAt)
            .Build();
}

[GenerateOneOf]
public partial class SearchDialogResult : OneOfBase<PaginatedList<SearchDialogDto>, ValidationError, Forbidden>;

internal sealed class SearchDialogQueryHandler : IRequestHandler<SearchDialogQuery, SearchDialogResult>
{
    private readonly IDialogDbContext _db;
    private readonly IMapper _mapper;
    private readonly IClock _clock;
    private readonly IUserService _userService;
    private readonly IAltinnAuthorization _altinnAuthorization;

    public SearchDialogQueryHandler(
        IDialogDbContext db,
        IMapper mapper,
        IClock clock,
        IUserService userService,
        IAltinnAuthorization altinnAuthorization)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _altinnAuthorization = altinnAuthorization ?? throw new ArgumentNullException(nameof(altinnAuthorization));
    }

    public async Task<SearchDialogResult> Handle(SearchDialogQuery request, CancellationToken cancellationToken)
    {
        if (!_userService.TryGetCurrentUserPid(out var userPid))
        {
            return new Forbidden("No valid user pid found.");
        }

        var searchExpression = Expressions.LocalizedSearchExpression(request.Search, request.SearchCultureCode);
        var authorizedResources = await _altinnAuthorization.GetAuthorizedResourcesForSearch(
            request.Party ?? [],
            request.ServiceResource ?? [],
            cancellationToken: cancellationToken);

        if (authorizedResources.HasNoAuthorizations)
        {
            return new PaginatedList<SearchDialogDto>(Enumerable.Empty<SearchDialogDto>(), false, null, request.OrderBy.DefaultIfNull().GetOrderString());
        }

        var paginatedList = await _db.Dialogs
            .AsNoTracking()
            .WhereUserIsAuthorizedFor(authorizedResources)
            .WhereIf(!request.Org.IsNullOrEmpty(), x => request.Org!.Contains(x.Org))
            .WhereIf(!request.ServiceResource.IsNullOrEmpty(), x => request.ServiceResource!.Contains(x.ServiceResource))
            .WhereIf(!request.Party.IsNullOrEmpty(), x => request.Party!.Contains(x.Party))
            .WhereIf(!request.ExtendedStatus.IsNullOrEmpty(), x => x.ExtendedStatus != null && request.ExtendedStatus!.Contains(x.ExtendedStatus))
            .WhereIf(!string.IsNullOrWhiteSpace(request.ExternalReference),
                x => x.ExternalReference != null && request.ExternalReference == x.ExternalReference)
            .WhereIf(!request.Status.IsNullOrEmpty(), x => request.Status!.Contains(x.StatusId))
            .WhereIf(request.CreatedAfter.HasValue, x => request.CreatedAfter <= x.CreatedAt)
            .WhereIf(request.CreatedBefore.HasValue, x => x.CreatedAt <= request.CreatedBefore)
            .WhereIf(request.UpdatedAfter.HasValue, x => request.UpdatedAfter <= x.UpdatedAt)
            .WhereIf(request.UpdatedBefore.HasValue, x => x.UpdatedAt <= request.UpdatedBefore)
            .WhereIf(request.DueAfter.HasValue, x => request.DueAfter <= x.DueAt)
            .WhereIf(request.DueBefore.HasValue, x => x.DueAt <= request.DueBefore)
            .WhereIf(request.Search is not null, x =>
                x.Content.Any(x => x.Value.Localizations.AsQueryable().Any(searchExpression)) ||
                x.SearchTags.Any(x => EF.Functions.ILike(x.Value, request.Search!))
            )
            .Where(x => !x.VisibleFrom.HasValue || _clock.UtcNowOffset > x.VisibleFrom)
            .Where(x => !x.ExpiresAt.HasValue || x.ExpiresAt > _clock.UtcNowOffset)
            .ProjectTo<SearchDialogDto>(_mapper.ConfigurationProvider)
            .ToPaginatedListAsync(request, cancellationToken: cancellationToken);

        await FetchRelevantActivities(paginatedList, userPid, cancellationToken);
        
        return paginatedList;
    }

    private async Task FetchRelevantActivities(PaginatedList<SearchDialogDto> paginatedList, string userPid, CancellationToken cancellationToken)
    {
        var dialogIds = paginatedList.Items
            .Select(x => x.Id)
            .ToList();

        var latestActivityByDialogIdTask = _db.DialogActivities
            .AsNoTracking()
            .Where(x =>
                dialogIds.Contains(x.DialogId)
                && x.TypeId != DialogActivityType.Values.Forwarded
                && x.TypeId != DialogActivityType.Values.Seen)
            .GroupBy(x => x.DialogId)
            .ToDictionaryAsync(
                x => x.Key,
                x => x.OrderByDescending(x => x.CreatedAt)
                    .ThenBy(x => x.Id)
                    .First(),
                cancellationToken);

        var latestSeenActivityByDialogIdTask = _db.DialogActivities
            .AsNoTracking()
            .Where(x =>
                dialogIds.Contains(x.DialogId)
                && x.TypeId == DialogActivityType.Values.Seen
                && x.CreatedAt > x.Dialog.UpdatedAt)
            .GroupBy(x => x.DialogId)
            .ToDictionaryAsync(x => x.Key, x => x.ToList(), cancellationToken);

        await Task.WhenAll(latestActivityByDialogIdTask, latestSeenActivityByDialogIdTask);

        var salt = MappingUtils.GetHashSalt();
        foreach (var dialog in paginatedList.Items)
        {
            var activities = latestSeenActivityByDialogIdTask.Result.TryGetValue(dialog.Id, out var seenActivities)
                ? seenActivities
                : new List<DialogActivity>();

            if (latestActivityByDialogIdTask.Result.TryGetValue(dialog.Id, out var latestNonSeenActivity))
            {
                activities.Add(latestNonSeenActivity);
            }

            dialog.LatestActivities = _mapper.Map<List<SearchDialogDialogActivityDto>>(activities);

            foreach (var activity in dialog.LatestActivities)
            {
                if (string.IsNullOrWhiteSpace(activity.SeenByEndUserIdHash))
                {
                    continue;
                }

                // Before we hash the end user id, check if the current user has seen the dialog
                activity.SeenActivityIsCurrentEndUser = userPid == activity.SeenByEndUserIdHash;
                // Hash end user ids
                activity.SeenByEndUserIdHash = MappingUtils.HashPid(activity.SeenByEndUserIdHash, salt);
            }
        }
    }
}
