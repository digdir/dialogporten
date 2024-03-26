using System.Diagnostics;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.Authorization;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Common.Pagination;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.GetQueryable;

public sealed class GetDialogQuery : IRequest<IQueryable<GetQueryableDialogDto>>
{
    public string? Search { get; set; }

    public string? SearchCultureCode { get; set; }

    ///// <summary>
    ///// Filter by one or more service resources
    ///// </summary>
    //public List<string>? ServiceResource { get; init; }

    ///// <summary>
    ///// Filter by one or more owning parties
    ///// </summary>
    //public List<string>? Party { get; init; }
}

internal sealed class GetDialogQueryableHandler : IRequestHandler<GetDialogQuery, IQueryable<GetQueryableDialogDto>>
{
    private readonly IDialogDbContext _db;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly IUserNameRegistry _userNameRegistry;
    private readonly IAltinnAuthorization _altinnAuthorization;

    public GetDialogQueryableHandler(
        IDialogDbContext db,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        IClock clock,
        IUserNameRegistry userNameRegistry,
        IAltinnAuthorization altinnAuthorization)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _userNameRegistry = userNameRegistry ?? throw new ArgumentNullException(nameof(userNameRegistry));
        _altinnAuthorization = altinnAuthorization ?? throw new ArgumentNullException(nameof(altinnAuthorization));
    }

    public Task<IQueryable<GetQueryableDialogDto>> Handle(GetDialogQuery request, CancellationToken cancellationToken)
    {
        var searchExpression = Expressions.LocalizedSearchExpression(request.Search, request.SearchCultureCode);

        var queryable = _db.Dialogs
            .WhereIf(request.Search is not null, x =>
                x.Content.Any(x => x.Value.Localizations.AsQueryable().Any(searchExpression)) ||
                x.SearchTags.Any(x => EF.Functions.ILike(x.Value, request.Search!))
            )
            .Where(x => !x.VisibleFrom.HasValue || _clock.UtcNowOffset > x.VisibleFrom)
            .Where(x => !x.ExpiresAt.HasValue || x.ExpiresAt > _clock.UtcNowOffset)
            .ProjectTo<GetQueryableDialogDto>(_mapper.ConfigurationProvider);

        return Task.FromResult(queryable);
        //var userName = await _userNameRegistry.GetCurrentUserName(userPid, cancellationToken);
        //// TODO: What if name lookup fails
        //// https://github.com/digdir/dialogporten/issues/387
        //dialog.UpdateSeenAt(userPid, userName);

        //var saveResult = await _unitOfWork
        //    .WithoutAuditableSideEffects()
        //    .SaveChangesAsync(cancellationToken);

        //saveResult.Switch(
        //    success => { },
        //    domainError => throw new UnreachableException("Should not get domain error when updating SeenAt."),
        //    concurrencyError => throw new UnreachableException("Should not get concurrencyError when updating SeenAt."));

        // hash end user ids
        //var salt = MappingUtils.GetHashSalt();
        //foreach (var activity in dialog.Activities)
        //{
        //    activity.SeenByEndUserId = MappingUtils.HashPid(activity.SeenByEndUserId, salt);
        //}

        //var dto = _mapper.Map<GetDialogDto>(dialog);

        //DecorateWithAuthorization(dto, authorizationResult);

        //return dto;
    }

    //private static void DecorateWithAuthorization(GetDialogDto dto, DialogDetailsAuthorizationResult authorizationResult)
    //{
    //    foreach (var (action, resources) in authorizationResult.AuthorizedAltinnActions)
    //    {
    //        foreach (var apiAction in dto.ApiActions.Where(a => a.Action == action))
    //        {
    //            if ((apiAction.AuthorizationAttribute is null && resources.Contains(Constants.MainResource))
    //                || (apiAction.AuthorizationAttribute is not null && resources.Contains(apiAction.AuthorizationAttribute)))
    //            {
    //                apiAction.IsAuthorized = true;
    //            }
    //        }

    //        foreach (var guiAction in dto.GuiActions.Where(a => a.Action == action))
    //        {
    //            if ((guiAction.AuthorizationAttribute is null && resources.Contains(Constants.MainResource))
    //                || (guiAction.AuthorizationAttribute is not null && resources.Contains(guiAction.AuthorizationAttribute)))
    //            {
    //                guiAction.IsAuthorized = true;
    //            }
    //        }

    //        // Simple "read" on the main resource will give access to a dialog element, unless a authorization attribute is set,
    //        // in which case an "elementread" action is required
    //        foreach (var dialogElement in dto.Elements.Where(
    //                     dialogElement => (dialogElement.AuthorizationAttribute is null)
    //                                      || (dialogElement.AuthorizationAttribute is not null
    //                                          && action == Constants.ElementReadAction)))
    //        {
    //            dialogElement.IsAuthorized = true;
    //        }
    //    }
    //}
}
