﻿using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Commands.Delete;

public sealed class DeleteDialogCommand : IRequest<OneOf<Success, EntityNotFound, UpdateConcurrencyError>>
{
    public Guid Id { get; set; }
    public Guid? ETag { get; set; }
}

internal sealed class DeleteDialogCommandHandler : IRequestHandler<DeleteDialogCommand, OneOf<Success, EntityNotFound, UpdateConcurrencyError>>
{
    private readonly IDialogDbContext _db;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventPublisher _eventPublisher;

    public DeleteDialogCommandHandler(IDialogDbContext db, IUnitOfWork unitOfWork, IDomainEventPublisher eventPublisher)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
    }

    public async Task<OneOf<Success, EntityNotFound, UpdateConcurrencyError>> Handle(DeleteDialogCommand request, CancellationToken cancellationToken)
    {
        var dialog = await _db.Dialogs
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (dialog is null)
        {
            return new EntityNotFound<DialogEntity>(request.Id);
        }

        _db.TrySetOriginalETag(dialog, request.ETag);

        // TODO: Delete localization sets
        _db.Dialogs.Remove(dialog);
        _eventPublisher.Publish(
            new DialogDeletedDomainEvent(
                dialog.Id, 
                dialog.ServiceResource.ToString(), 
                dialog.Party));

        var saveResult = await _unitOfWork.SaveChangesAsync(cancellationToken);
        return saveResult.Match<OneOf<Success, EntityNotFound, UpdateConcurrencyError>>(
            success => success,
            domainError => throw new ApplicationException("Should never get a domain error when creating a new dialog"),
            concurrencyError => concurrencyError);
    }
}
