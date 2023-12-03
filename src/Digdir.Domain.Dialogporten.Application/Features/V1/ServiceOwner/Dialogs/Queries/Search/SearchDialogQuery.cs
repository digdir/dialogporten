using AutoMapper;
using AutoMapper.QueryableExtensions;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Common.Extensions.Enumerable;
using Digdir.Domain.Dialogporten.Application.Common.Pagination;
using Digdir.Domain.Dialogporten.Application.Common.Pagination.OrderOption;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using MediatR;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Search;

public sealed class SearchDialogQuery : SortablePaginationParameter<SearchDialogQueryOrderDefinition, SearchDialogDto>, IRequest<SearchDialogResult>
{
    private string? _searchCultureCode;

    public List<string>? ServiceResource { get; init; }
    public List<string>? Party { get; init; }
    public List<string>? ExtendedStatus { get; init; }
    public List<DialogStatus.Values>? Status { get; init; }

    public DateTimeOffset? CreatedAfter { get; init; }
    public DateTimeOffset? CreatedBefore { get; init; }

    public DateTimeOffset? UpdatedAfter { get; init; }
    public DateTimeOffset? UpdatedBefore { get; init; }

    public DateTimeOffset? DueAfter { get; init; }
    public DateTimeOffset? DueBefore { get; init; }

    public DateTimeOffset? VisibleAfter { get; init; }
    public DateTimeOffset? VisibleBefore { get; init; }

    public string? Search { get; init; }
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
public partial class SearchDialogResult : OneOfBase<PaginatedList<SearchDialogDto>, ValidationError> { }

internal sealed class SearchDialogQueryHandler : IRequestHandler<SearchDialogQuery, SearchDialogResult>
{
    private readonly IDialogDbContext _db;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;

    public SearchDialogQueryHandler(
        IDialogDbContext db,
        IMapper mapper,
        IUserService userService)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public async Task<SearchDialogResult> Handle(SearchDialogQuery request, CancellationToken cancellationToken)
    {
        var resourceIds = await _userService.GetCurrentUserResourceIds(cancellationToken);
        var searchExpression = Expressions.LocalizedSearchExpression(request.Search, request.SearchCultureCode);
        return await _db.Dialogs
            .WhereIf(!request.ServiceResource.IsNullOrEmpty(), x => request.ServiceResource!.Contains(x.ServiceResource))
            .WhereIf(!request.Party.IsNullOrEmpty(), x => request.Party!.Contains(x.Party))
            .WhereIf(!request.ExtendedStatus.IsNullOrEmpty(), x => x.ExtendedStatus != null && request.ExtendedStatus!.Contains(x.ExtendedStatus))
            .WhereIf(!request.Status.IsNullOrEmpty(), x => request.Status!.Contains(x.StatusId))
            .WhereIf(request.CreatedAfter.HasValue, x => request.CreatedAfter <= x.CreatedAt)
            .WhereIf(request.CreatedBefore.HasValue, x => x.CreatedAt <= request.CreatedBefore)
            .WhereIf(request.UpdatedAfter.HasValue, x => request.UpdatedAfter <= x.UpdatedAt)
            .WhereIf(request.UpdatedBefore.HasValue, x => x.UpdatedAt <= request.UpdatedBefore)
            .WhereIf(request.DueAfter.HasValue, x => request.DueAfter <= x.DueAt)
            .WhereIf(request.DueBefore.HasValue, x => x.DueAt <= request.DueBefore)
            .WhereIf(request.VisibleAfter.HasValue, x => request.VisibleAfter <= x.VisibleFrom)
            .WhereIf(request.VisibleBefore.HasValue, x => x.VisibleFrom <= request.VisibleBefore)
            .WhereIf(request.Search is not null, x =>
                x.Title!.Localizations.AsQueryable().Any(searchExpression) ||
                x.Body!.Localizations.AsQueryable().Any(searchExpression) ||
                x.SearchTags.Any(x => x.Value.Equals(request.Search, StringComparison.OrdinalIgnoreCase)) ||
                x.SenderName!.Localizations.AsQueryable().Any(searchExpression)
            )
            .Where(x => resourceIds.Contains(x.ServiceResource))
            .ProjectTo<SearchDialogDto>(_mapper.ConfigurationProvider)
            .ToPaginatedListAsync(request, cancellationToken: cancellationToken);
    }
}
