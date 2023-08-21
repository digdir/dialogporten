using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Get;

public sealed class GetDialogQuery : IRequest<GetDialogResult>
{
    public Guid Id { get; set; }
}

[GenerateOneOf]
public partial class GetDialogResult : OneOfBase<GetDialogDto, EntityNotFound, EntityDeleted> { }

internal sealed class GetDialogQueryHandler : IRequestHandler<GetDialogQuery, GetDialogResult>
{
    private readonly IDialogDbContext _db;
    private readonly IMapper _mapper;
    private readonly IDomainEventPublisher _eventPublisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITransactionTime _transactionTime;
    private readonly IClock _clock;

    public GetDialogQueryHandler(
        IDialogDbContext db,
        IMapper mapper,
        IDomainEventPublisher eventPublisher,
        IUnitOfWork unitOfWork,
        ITransactionTime transactionTime,
        IClock clock)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _transactionTime = transactionTime ?? throw new ArgumentNullException(nameof(transactionTime));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<GetDialogResult> Handle(GetDialogQuery request, CancellationToken cancellationToken)
    {
        // This query could be written without all the includes as ProjctTo will do the job for us.
        // However, we need to guarantee an order for sub resources of the dialog aggragate.
        // This is to ensure that the get is consistent, and that PATCH in the API presentation
        // layer behavious in an expected manner. Therefore we need to be a bit more verbose about it.
        var dialog = await _db.Dialogs
            .Include(x => x.Body!.Localizations.OrderBy(x => x.CreatedAt).ThenBy(x => x.CultureCode))
            .Include(x => x.Title!.Localizations.OrderBy(x => x.CreatedAt).ThenBy(x => x.CultureCode))
            .Include(x => x.SenderName!.Localizations.OrderBy(x => x.CreatedAt).ThenBy(x => x.CultureCode))
            .Include(x => x.SearchTitle!.Localizations.OrderBy(x => x.CreatedAt).ThenBy(x => x.CultureCode))
            .Include(x => x.Elements.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
                .ThenInclude(x => x.DisplayName!.Localizations.OrderBy(x => x.CreatedAt).ThenBy(x => x.CultureCode))
            .Include(x => x.Elements.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
                .ThenInclude(x => x.Urls.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
            .Include(x => x.GuiActions.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
                .ThenInclude(x => x.Title!.Localizations.OrderBy(x => x.CreatedAt).ThenBy(x => x.CultureCode))
            .Include(x => x.ApiActions.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
                .ThenInclude(x => x.Endpoints.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
            .Where(x => !x.VisibleFrom.HasValue || x.VisibleFrom < _clock.UtcNowOffset)
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (dialog is null)
        {
            return new EntityNotFound<DialogEntity>(request.Id);
        }

        if (dialog.Deleted)
        {
            return new EntityDeleted<DialogEntity>(request.Id);
        }

        if ((dialog.ReadAt ?? DateTimeOffset.MinValue) < dialog.UpdatedAt)
        {
            dialog.ReadAt = await UpdateReadAt(request.Id, cancellationToken);
        }

        var dto = _mapper.Map<GetDialogDto>(dialog);

        return dto;
    }

    private async Task<DateTimeOffset?> UpdateReadAt(Guid dialogId, CancellationToken cancellationToken)
    {
        var modifiableDialog = await _db.Dialogs.FindAsync(new object[] { dialogId }, cancellationToken: cancellationToken);
        _eventPublisher.Publish(new DialogReadDomainEvent(dialogId));
        modifiableDialog!.ReadAt = _transactionTime.Value;
        var saveResult = await _unitOfWork
            .WithoutAuditableSideEffects()
            .SaveChangesAsync(cancellationToken);
        saveResult.Switch(
            success => { },
            domainError => throw new ApplicationException("Should not get domain error when updating ReadAt."),
            concurrencyError => throw new ApplicationException("Should not get concurrencyError when updating ReadAt."));
        return modifiableDialog.ReadAt;
    }
}