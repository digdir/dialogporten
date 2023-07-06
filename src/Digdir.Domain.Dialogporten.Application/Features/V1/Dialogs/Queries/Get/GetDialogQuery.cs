using AutoMapper;
using AutoMapper.QueryableExtensions;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Queries.Get;

public sealed class GetDialogQuery : IRequest<OneOf<GetDialogDto, EntityNotFound>>
{
    public Guid Id { get; set; }
}

internal sealed class GetDialogQueryHandler : IRequestHandler<GetDialogQuery, OneOf<GetDialogDto, EntityNotFound>>
{
    private readonly IDialogDbContext _db;
    private readonly IMapper _mapper;
    private readonly IDomainEventPublisher _eventPublisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITransactionTime _transactionTime;

    public GetDialogQueryHandler(
        IDialogDbContext db,
        IMapper mapper,
        IDomainEventPublisher eventPublisher,
        IUnitOfWork unitOfWork,
        ITransactionTime transactionTime)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _transactionTime = transactionTime ?? throw new ArgumentNullException(nameof(transactionTime));
    }

    public async Task<OneOf<GetDialogDto, EntityNotFound>> Handle(GetDialogQuery request, CancellationToken cancellationToken)
    {
        // This query could be written without all the includes as ProjctTo will do the job for us.
        // However, we need to guarantee an order for sub resources of the dialog aggragate.
        // This is to ensure that the get is consistent, and that PATCH in the API presentation
        // layer behavious in an expected manner. Therefore we need to be a bit more verbose about it.
        var dialog = await _db.Dialogs
            .Include(x => x.Body.Localizations.OrderBy(x => x.CreatedAt).ThenBy(x => x.CultureCode))
            .Include(x => x.Title.Localizations.OrderBy(x => x.CreatedAt).ThenBy(x => x.CultureCode))
            .Include(x => x.SenderName.Localizations.OrderBy(x => x.CreatedAt).ThenBy(x => x.CultureCode))
            .Include(x => x.SearchTitle.Localizations.OrderBy(x => x.CreatedAt).ThenBy(x => x.CultureCode))
            .Include(x => x.Elements.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
                .ThenInclude(x => x.DisplayName.Localizations.OrderBy(x => x.CreatedAt))
            .Include(x => x.Elements.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
                .ThenInclude(x => x.Urls.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
            .Include(x => x.GuiActions.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
                .ThenInclude(x => x.Title.Localizations.OrderBy(x => x.CreatedAt).ThenBy(x => x.CultureCode))
            .Include(x => x.ApiActions.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
                .ThenInclude(x => x.Endpoints.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
            .AsNoTracking()
            .ProjectTo<GetDialogDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (dialog is null)
        {
            return new EntityNotFound<DialogEntity>(request.Id);
        }

        if ((dialog.ReadAt ?? DateTimeOffset.MinValue) < dialog.UpdatedAt)
        {
            // TODO: Should we only do this if the user is an end user?
            var mutableDialog = await _db.Dialogs.FindAsync(new object[] { request.Id }, cancellationToken);
            _eventPublisher.Publish(new DialogReadDomainEvent(request.Id));
            mutableDialog!.ReadAt = _transactionTime.Value;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return dialog;
    }
}