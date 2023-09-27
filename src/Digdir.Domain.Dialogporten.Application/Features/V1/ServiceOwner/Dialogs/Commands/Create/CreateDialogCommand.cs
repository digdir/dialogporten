﻿using System.Diagnostics;
using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;
using Digdir.Domain.Dialogporten.Domain.Common;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events;
using Digdir.Library.Entity.Abstractions.Features.Identifiable;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Create;

public sealed class CreateDialogCommand : CreateDialogDto, IRequest<CreateDialogResult> { }

[GenerateOneOf]
public partial class CreateDialogResult : OneOfBase<Success<Guid>, DomainError, ValidationError> { }

internal sealed class CreateDialogCommandHandler : IRequestHandler<CreateDialogCommand, CreateDialogResult>
{
    private readonly IDialogDbContext _db;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventPublisher _eventPublisher;
    private readonly IDomainContext _domainContext;
    private readonly IResourceRegistry _resourceRegistry;
    private readonly IUser _user;

    public CreateDialogCommandHandler(
        IDialogDbContext db,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        IDomainEventPublisher eventPublisher,
        IDomainContext domainContext,
        IUser user,
        IResourceRegistry resourceRegistry)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _domainContext = domainContext ?? throw new ArgumentNullException(nameof(domainContext));
        _user = user ?? throw new ArgumentNullException(nameof(user));
        _resourceRegistry = resourceRegistry ?? throw new ArgumentNullException(nameof(resourceRegistry));
    }

    public async Task<CreateDialogResult> Handle(CreateDialogCommand request, CancellationToken cancellationToken)
    {
        var ownerOrgNumber = await _resourceRegistry
            .GetOrgOwner(request.ServiceResource.ToString(), cancellationToken);

        if (!_user.TryGetOrgNumber(out var orgNumber))
        {
            throw new UnreachableException();
        }

        if (ownerOrgNumber != orgNumber)
        {
            // TODO: Return Unauthorized
            throw new Exception();
        }

        var dialog = _mapper.Map<DialogEntity>(request);

        var existingDialogIds = await _db.GetExistingIds(new[] { dialog }, cancellationToken);
        if (existingDialogIds.Any())
        {
            _domainContext.AddError(DomainFailure.EntiryExists<DialogEntity>(existingDialogIds));
        }

        var existingActivityIds = await _db.GetExistingIds(dialog.Activities, cancellationToken);
        if (existingActivityIds.Any())
        {
            _domainContext.AddError(DomainFailure.EntiryExists<DialogActivity>(existingActivityIds));
        }

        var existingElementIds = await _db.GetExistingIds(dialog.Elements, cancellationToken);
        if (existingElementIds.Any())
        {
            _domainContext.AddError(DomainFailure.EntiryExists<DialogElement>(existingElementIds));
        }

        await _db.Dialogs.AddAsync(dialog, cancellationToken);
        _eventPublisher.Publish(new DialogCreatedDomainEvent(dialog.CreateId()));
        _eventPublisher.Publish(dialog.Activities.Select(x => new DialogActivityCreatedDomainEvent(dialog.Id, x.CreateId())));

        var saveResult = await _unitOfWork.SaveChangesAsync(cancellationToken);
        return saveResult.Match<CreateDialogResult>(
            success => new Success<Guid>(dialog.Id),
            domainError => domainError,
            concurrencyError => throw new UnreachableException("Should never get a concurrency error when creating a new dialog"));
    }
}