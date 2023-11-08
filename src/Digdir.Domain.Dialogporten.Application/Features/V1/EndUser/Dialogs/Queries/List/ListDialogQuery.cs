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
using static Digdir.Domain.Dialogporten.Application.Common.Expressions;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.List;

public sealed class ListDialogQuery : SortablePaginationParameter<ListDialogQueryOrderDefinition, ListDialogDto>, IRequest<ListDialogResult>
{
    private string? _searchCultureCode;

    public List<string>? Org { get; init; }
    public List<string>? ServiceResource { get; init; }
    public List<string>? Party { get; init; }
    public List<string>? ExtendedStatus { get; init; }
    public List<DialogStatus.Enum>? Status { get; init; }

    public DateTimeOffset? CreatedAfter { get; init; }
    public DateTimeOffset? CreatedBefore { get; init; }

    public DateTimeOffset? UpdatedAfter { get; init; }
    public DateTimeOffset? UpdatedBefore { get; init; }

    public DateTimeOffset? DueAfter { get; init; }
    public DateTimeOffset? DueBefore { get; init; }

    public string? Search { get; init; }
    public string? SearchCultureCode 
    { 
        get => _searchCultureCode; 
        init => _searchCultureCode = Localization.NormalizeCultureCode(value); 
    }
}

public sealed class ListDialogQueryOrderDefinition : IOrderDefinition<ListDialogDto>
{
    public static IOrderOptions<ListDialogDto> Configure(IOrderOptionsBuilder<ListDialogDto> options) =>
        options.AddId(x => x.Id)
            .AddDefault("createdAt", x => x.CreatedAt)
            .AddOption("updatedAt", x => x.UpdatedAt)
            .AddOption("dueAt", x => x.DueAt)
            .Build();
}

[GenerateOneOf]
public partial class ListDialogResult : OneOfBase<PaginatedList<ListDialogDto>, ValidationError> { }

internal sealed class ListDialogQueryHandler : IRequestHandler<ListDialogQuery, ListDialogResult>
{
    private readonly IDialogDbContext _db;
    private readonly IMapper _mapper;
    private readonly IClock _clock;

    public ListDialogQueryHandler(IDialogDbContext db, IMapper mapper, IClock clock)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<ListDialogResult> Handle(ListDialogQuery request, CancellationToken cancellationToken)
    {
        var searchExpression = LocalizedSearchExpression(request.Search, request.SearchCultureCode);

        return await _db.Dialogs
            .WhereIf(!request.Org.IsNullOrEmpty(), x => request.Org!.Contains(x.Org))
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
            .WhereIf(request.Search is not null, x =>
                x.Title!.Localizations.AsQueryable().Any(searchExpression) ||
                x.SearchTags.Any(x => x.Value == request.Search!.ToLower()) ||
                x.SenderName!.Localizations.AsQueryable().Any(searchExpression)
            )
            .Where(x => !x.VisibleFrom.HasValue || _clock.UtcNowOffset > x.VisibleFrom)
            .Where(x => !x.ExpiresAt.HasValue || x.ExpiresAt > _clock.UtcNowOffset)
            .ProjectTo<ListDialogDto>(_mapper.ConfigurationProvider)
            .ToPaginatedListAsync(request, cancellationToken: cancellationToken);
    }
}
