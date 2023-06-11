using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events;
using Digdir.Library.Entity.Abstractions.Features.Identifiable;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Commands.Create;

public sealed class CreateDialogCommand : CreateDialogDto, IRequest<OneOf<Guid, EntityExists, ValidationError>> { }

internal sealed class CreateDialogCommandHandler : IRequestHandler<CreateDialogCommand, OneOf<Guid, EntityExists, ValidationError>>
{
    private readonly IDialogDbContext _db;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventPublisher _eventPublisher;

    public CreateDialogCommandHandler(IDialogDbContext db, IMapper mapper, IUnitOfWork unitOfWork, IDomainEventPublisher eventPublisher)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
    }

    public async Task<OneOf<Guid, EntityExists, ValidationError>> Handle(CreateDialogCommand request, CancellationToken cancellationToken)
    {
        if (await IsExistingDialogId(request.Id, cancellationToken))
        {
            return new EntityExists<DialogEntity>(request.Id!.Value);
        }

        // We need to populate the Ids of the elements and activities before we map to the entity
        // so we can match the DTO and entity Ids when handling internal relationships below
        PopulateDialogElementIds(request.Elements);
        PopulateDialogActivityIds(request.History);

        var dialog = _mapper.Map<DialogEntity>(request);

        // Internal relationships (dialogelement -> dialogelement, activity -> activity, activity -> dialogelement)
        // are handled below. We allow specifying activities/dialogelements that are not part of the payload but in
        // the database, if they are associated with the same dialog.
        var validationError = await HandleDialogElementRelationships(request.Elements, dialog);
        if (validationError is not null) return validationError;

        validationError = await HandleActivityToActivityRelationships(request.History, dialog);
        if (validationError is not null) return validationError;

        validationError = await HandleActivityToDialogElementRelationships(request.History, dialog);
        if (validationError is not null) return validationError;

        await _db.Dialogs.AddAsync(dialog, cancellationToken);
        _eventPublisher.Publish(new DialogCreatedDomainEvent(dialog.CreateId()));
        _eventPublisher.Publish(dialog.History.Select(x => new DialogActivityCreatedDomainEvent(dialog.Id, x.CreateId())));
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return dialog.Id;
    }

    private static void PopulateDialogElementIds(List<CreateDialogDialogElementDto> requestElements) =>
        requestElements.ForEach(x => x.Id ??= Guid.NewGuid());

    private static void PopulateDialogActivityIds(List<CreateDialogDialogActivityDto> requestActivities) =>
        requestActivities.ForEach(x => x.Id ??= Guid.NewGuid());

    private async Task<bool> IsExistingDialogId(Guid? id, CancellationToken cancellationToken) =>
        id.HasValue && await _db.Dialogs.AnyAsync(x => x.Id == id, cancellationToken);

    private async Task<ValidationError?> HandleDialogElementRelationships(List<CreateDialogDialogElementDto> elementsDto, DialogEntity dialog)
    {
        foreach (var elementDto in elementsDto)
        {
            var element = dialog.Elements.FirstOrDefault(e => e.Id == elementDto.Id);
            if (element is null) throw new InvalidOperationException($"Mapping broken! element with id {elementDto.Id} not found in mapped entity");

            if (!elementDto.RelatedDialogElementId.HasValue) continue;
            var relatedElement = dialog.Elements.FirstOrDefault(e => e.Id == elementDto.RelatedDialogElementId.Value);

            if (relatedElement is not null)
            {
                element.RelatedDialogElement = relatedElement;
                element.RelatedDialogElementInternalId = relatedElement.InternalId;
            }
            else
            {
                // TODO! Check soft-deletion?
                var dbElement = await _db.DialogElements
                    .Where(e => e.Id == elementDto.RelatedDialogElementId.Value && e.DialogId == dialog.InternalId)
                    .SingleOrDefaultAsync();

                if (dbElement is not null)
                {
                    element.RelatedDialogElement = dbElement;
                    element.RelatedDialogElementInternalId = dbElement.InternalId;
                }
                else
                {
                    var validationFailure = new ValidationFailure("RelatedDialogElementId",
                        $"A dialog element attempted to refer to a dialog element with id {elementDto.RelatedDialogElementId}, which does not exist or is not assoicated with this dialog");
                    return new ValidationError(validationFailure);
                }
            }
        }

        return null;
    }

    private async Task<ValidationError?> HandleActivityToActivityRelationships(List<CreateDialogDialogActivityDto> activitiesDto, DialogEntity dialog)
    {
        foreach (var activityDto in activitiesDto)
        {
            var activity = dialog.History.FirstOrDefault(a => a.Id == activityDto.Id);
            if (activity is null) throw new InvalidOperationException($"Mapping broken! activity with id {activityDto.Id} does not exist in mapped entity");

            if (!activityDto.RelatedActivityId.HasValue) continue;
            var relatedActivity = dialog.History.FirstOrDefault(a => a.Id == activityDto.RelatedActivityId.Value);
            if (relatedActivity is not null)
            {
                activity.RelatedActivity = relatedActivity;
                activity.RelatedActivityInternalId = relatedActivity.InternalId;
            }
            else
            {
                // TODO! Check soft-deletion?
                var dbActivity = await _db.DialogActivities
                    .Where(a => a.Id == activityDto.RelatedActivityId.Value && a.DialogId == dialog.InternalId)
                    .SingleOrDefaultAsync();

                if (dbActivity is not null)
                {
                    activity.RelatedActivity = dbActivity;
                    activity.RelatedActivityInternalId = dbActivity.InternalId;
                }
                else
                {
                    var validationFailure = new ValidationFailure("RelatedActivityId",
                        $"An activity attempted to refer a related activity with id {activityDto.RelatedActivityId}, which does not exist or is not assoicated with this dialog");
                    return new ValidationError(validationFailure);
                }
            }
        }

        return null;
    }

    private async Task<ValidationError?> HandleActivityToDialogElementRelationships(List<CreateDialogDialogActivityDto> activitiesDto, DialogEntity dialog)
    {
        foreach (var activityDto in activitiesDto)
        {
            var activity = dialog.History.FirstOrDefault(a => a.Id == activityDto.Id);
            if (activity is null) throw new InvalidOperationException($"Mapping broken! activity with id {activityDto.Id} does not exist in mapped entity");

            if (!activityDto.DialogElementId.HasValue) continue;
            var relatedElement = dialog.Elements.FirstOrDefault(e => e.Id == activityDto.DialogElementId.Value);
            if (relatedElement is not null)
            {
                activity.DialogElement = relatedElement;
                activity.DialogElementInternalId = relatedElement.InternalId;
            }
            else
            {
                // TODO! Check soft-deletion?
                var dbElement = await _db.DialogElements
                    .Where(e => e.Id == activityDto.DialogElementId.Value && e.DialogId == dialog.InternalId)
                    .SingleOrDefaultAsync();

                if (dbElement is not null)
                {
                    activity.DialogElement = dbElement;
                    activity.DialogElementInternalId = dbElement.InternalId;
                }
                else
                {
                    var validationFailure = new ValidationFailure("DialogElementId",
                        $"An activity attempted to refer a dialogElement with id {activityDto.DialogElementId}, which does not exist or is not assoicated with this dialog");
                    return new ValidationError(validationFailure);
                }
            }
        }

        return null;
    }
}
