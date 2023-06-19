using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events;
using Digdir.Library.Entity.Abstractions.Features.Identifiable;
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
        if (await DialogExists(request.Id, cancellationToken))
        {
            return new EntityExists<DialogEntity>(request.Id!.Value);
        }

        // TODO: Validate no used Ids
        // TODO: Validate relationships
        var dialog = _mapper.Map<DialogEntity>(request);

        //ValidateRelationships(request, dialog);

        await _db.Dialogs.AddAsync(dialog, cancellationToken);
        _eventPublisher.Publish(new DialogCreatedDomainEvent(dialog.CreateId()));
        _eventPublisher.Publish(dialog.Activities.Select(x => new DialogActivityCreatedDomainEvent(dialog.Id, x.CreateId())));
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return dialog.Id;
    }

    //private static void ValidateRelationships(CreateDialogCommand request, DialogEntity dialog)
    //{


    //    var activityActivityPairs = request.Activities
    //        .Where(x => x.RelatedActivityId.HasValue)
    //        .ToDictionary(x => x.Id!.Value, x => x.RelatedActivityId!.Value)
    //        .JoinPairs(
    //            inner: dialog.Activities,
    //            outer: dialog.Activities,
    //            innerKeySelector: x => x.Id,
    //            outerKeySelector: x => x.Id);

    //    var activityElementPairs = request.Activities
    //        .Where(x => x.DialogElementId.HasValue)
    //        .ToDictionary(x => x.Id!.Value, x => x.DialogElementId!.Value)
    //        .JoinPairs(
    //            inner: dialog.Activities,
    //            outer: dialog.Elements,
    //            innerKeySelector: x => x.Id,
    //            outerKeySelector: x => x.Id);

    //    var elementElementPairs = request.Elements
    //        .Where(x => x.RelatedDialogElementId.HasValue)
    //        .ToDictionary(x => x.Id!.Value, x => x.RelatedDialogElementId!.Value)
    //        .JoinPairs(
    //            inner: dialog.Elements,
    //            outer: dialog.Elements,
    //            innerKeySelector: x => x.Id,
    //            outerKeySelector: x => x.Id);

    //    var apiActionElementPairs = request.ApiActions
    //        .Where(x => x.DialogElementId.HasValue)
    //        .ToDictionary(x => x.Id!.Value, x => x.DialogElementId!.Value)
    //        .JoinPairs(
    //            inner: dialog.ApiActions,
    //            outer: dialog.Elements,
    //            innerKeySelector: x => x.Id,
    //            outerKeySelector: x => x.Id);

    //    foreach (var (dependentActivity, principalActivity) in activityActivityPairs)
    //    {
    //        dependentActivity.RelatedActivity = principalActivity;
    //    }

    //    foreach (var (dependentActivity, principalElement) in activityElementPairs)
    //    {
    //        dependentActivity.DialogElement = principalElement;
    //    }

    //    foreach (var (dependentElement, principalElement) in elementElementPairs)
    //    {
    //        dependentElement.RelatedDialogElement = principalElement;
    //    }

    //    foreach (var (dependentApiAction, principalElement) in apiActionElementPairs)
    //    {
    //        dependentApiAction.DialogElement = principalElement;
    //    }
    //}

    private async Task<bool> DialogExists(Guid? id, CancellationToken cancellationToken) =>
        id.HasValue && await _db.Dialogs.AnyAsync(x => x.Id == id, cancellationToken);

    //private ValidationError? HandleDialogElementRelationships(List<CreateDialogDialogElementDto> elementsDto, DialogEntity dialog)
    //{
    //    foreach (var elementDto in elementsDto)
    //    {
    //        if (!elementDto.RelatedDialogElementId.HasValue) continue;
    //        var element = dialog.Elements.FirstOrDefault(e => e.Id == elementDto.Id);
    //        if (element is null) throw new InvalidOperationException($"Mapping broken! element with id {elementDto.Id} not found in mapped entity");
    //        var relatedElement = dialog.Elements.FirstOrDefault(e => e.Id == elementDto.RelatedDialogElementId.Value);

    //        if (relatedElement is null)
    //        {
    //            var validationFailure = new ValidationFailure("RelatedDialogElementId",
    //                $"A dialog element attempted to refer to a dialog element with id {elementDto.RelatedDialogElementId}, which is not assoicated with this dialog");
    //            return new ValidationError(validationFailure);
    //        }

    //        element.RelatedDialogElement = relatedElement;
    //    }

    //    return null;
    //}

    //private ValidationError? HandleActivityToActivityRelationships(List<CreateDialogDialogActivityDto> activitiesDto, DialogEntity dialog)
    //{
    //    foreach (var activityDto in activitiesDto)
    //    {
    //        if (!activityDto.RelatedActivityId.HasValue) continue;
    //        var activity = dialog.Activities.FirstOrDefault(a => a.Id == activityDto.Id);
    //        if (activity is null) throw new InvalidOperationException($"Mapping broken! activity with id {activityDto.Id} does not exist in mapped entity");
    //        var relatedActivity = dialog.Activities.FirstOrDefault(a => a.Id == activityDto.RelatedActivityId.Value);

    //        if (relatedActivity is null)
    //        {
    //            var validationFailure = new ValidationFailure("RelatedActivityId",
    //                $"An activity attempted to refer a related activity with id {activityDto.RelatedActivityId}, which is not assoicated with this dialog");
    //            return new ValidationError(validationFailure);
    //        }

    //        activity.RelatedActivity = relatedActivity;
    //    }

    //    return null;
    //}

    //private ValidationError? HandleActivityToDialogElementRelationships(List<CreateDialogDialogActivityDto> activitiesDto, DialogEntity dialog)
    //{
    //    foreach (var activityDto in activitiesDto)
    //    {
    //        if (!activityDto.DialogElementId.HasValue) continue;
    //        var activity = dialog.Activities.FirstOrDefault(a => a.Id == activityDto.Id);
    //        if (activity is null) throw new InvalidOperationException($"Mapping broken! activity with id {activityDto.Id} does not exist in mapped entity");
    //        var relatedElement = dialog.Elements.FirstOrDefault(e => e.Id == activityDto.DialogElementId.Value);

    //        if (relatedElement is null)
    //        {
    //            var validationFailure = new ValidationFailure("DialogElementId",
    //                $"An activity attempted to refer a dialogElement with id {activityDto.DialogElementId}, which does not exist or is not assoicated with this dialog");
    //            return new ValidationError(validationFailure);
    //        }

    //        activity.DialogElement = relatedElement;
    //    }

    //    return null;
    //}
}
