using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.Extensions.Enumerables;
using Digdir.Domain.Dialogporten.Application.Common.ResourceRegistry;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Attachments;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Update;

public sealed class UpdateDialogCommand : IRequest<UpdateDialogResult>
{
    public Guid Id { get; set; }
    public Guid? IfMatchDialogRevision { get; set; }
    public UpdateDialogDto Dto { get; set; } = null!;
}

[GenerateOneOf]
public partial class UpdateDialogResult : OneOfBase<Success, EntityNotFound, BadRequest, ValidationError, Forbidden, DomainError, ConcurrencyError>;

internal sealed class UpdateDialogCommandHandler : IRequestHandler<UpdateDialogCommand, UpdateDialogResult>
{
    private readonly IDialogDbContext _db;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainContext _domainContext;
    private readonly IUserResourceRegistry _userResourceRegistry;

    private readonly ValidationFailure _progressValidationFailure = new(nameof(UpdateDialogDto.Progress), "Progress cannot be set for correspondence dialogs.");

    public UpdateDialogCommandHandler(
        IDialogDbContext db,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        IDomainContext domainContext,
        IUserResourceRegistry userResourceRegistry)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _domainContext = domainContext ?? throw new ArgumentNullException(nameof(domainContext));
        _userResourceRegistry = userResourceRegistry ?? throw new ArgumentNullException(nameof(userResourceRegistry));
    }

    public async Task<UpdateDialogResult> Handle(UpdateDialogCommand request, CancellationToken cancellationToken)
    {
        var resourceIds = await _userResourceRegistry.GetCurrentUserResourceIds(cancellationToken);

        var dialog = await _db.Dialogs
            .Include(x => x.Activities)
            .Include(x => x.Content)
                .ThenInclude(x => x.Value.Localizations)
            .Include(x => x.SearchTags)
            .Include(x => x.Attachments)
                .ThenInclude(x => x.DisplayName!.Localizations)
            .Include(x => x.Attachments)
                .ThenInclude(x => x.Urls)
            .Include(x => x.GuiActions)
                .ThenInclude(x => x.Title!.Localizations)
            .Include(x => x.GuiActions)
                .ThenInclude(x => x!.Prompt!.Localizations)
            .Include(x => x.ApiActions)
                .ThenInclude(x => x.Endpoints)
            .Include(x => x.Transmissions)
            .IgnoreQueryFilters()
            .Where(x => resourceIds.Contains(x.ServiceResource))
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (dialog is null)
        {
            return new EntityNotFound<DialogEntity>(request.Id);
        }

        foreach (var serviceResourceReference in GetServiceResourceReferences(request.Dto))
        {
            if (!await _userResourceRegistry.CurrentUserIsOwner(serviceResourceReference, cancellationToken))
            {
                return new Forbidden($"Not allowed to reference {serviceResourceReference}.");
            }
        }

        if (!_userResourceRegistry.UserCanModifyResourceType(dialog.ServiceResourceType))
        {
            return new Forbidden($"User cannot modify resource type {dialog.ServiceResourceType}.");
        }

        if (dialog.ServiceResourceType == Constants.Correspondence)
        {
            if (request.Dto.Progress is not null)
                return new ValidationError(_progressValidationFailure);
        }

        if (dialog.Deleted)
        {
            // TODO: When restoration is implemented, add a hint to the error message.
            // https://github.com/digdir/dialogporten/pull/406
            return new BadRequest($"Entity '{nameof(DialogEntity)}' with key '{request.Id}' is removed, and cannot be updated.");
        }

        // Update primitive properties
        _mapper.Map(request.Dto, dialog);
        ValidateTimeFields(dialog);

        await AppendActivity(dialog, request.Dto, cancellationToken);
        VerifyActivityRelations(dialog);

        AppendTransmission(dialog, request.Dto);
        VerifyActivityTransmissionRelations(dialog);
        VerifyTransmissionRelations(dialog);

        dialog.SearchTags
            .Merge(request.Dto.SearchTags,
                destinationKeySelector: x => x.Value,
                sourceKeySelector: x => x.Value,
                create: _mapper.Map<List<DialogSearchTag>>,
                delete: DeleteDelegate.NoOp,
                comparer: StringComparer.InvariantCultureIgnoreCase);

        await dialog.Attachments
            .MergeAsync(request.Dto.Attachments,
                destinationKeySelector: x => x.Id,
                sourceKeySelector: x => x.Id,
                create: CreateAttachments,
                update: UpdateAttachments,
                delete: DeleteDelegate.NoOp,
                cancellationToken: cancellationToken);

        dialog.GuiActions
            .Merge(request.Dto.GuiActions,
                destinationKeySelector: x => x.Id,
                sourceKeySelector: x => x.Id,
                create: _mapper.Map<List<DialogGuiAction>>,
                update: _mapper.Update,
                delete: DeleteDelegate.NoOp);

        dialog.ApiActions
            .Merge(request.Dto.ApiActions,
                destinationKeySelector: x => x.Id,
                sourceKeySelector: x => x.Id,
                create: CreateApiActions,
                update: UpdateApiActions,
                delete: DeleteDelegate.NoOp);

        var saveResult = await _unitOfWork
            .EnableConcurrencyCheck(dialog, request.IfMatchDialogRevision)
            .SaveChangesAsync(cancellationToken);

        return saveResult.Match<UpdateDialogResult>(
            success => success,
            domainError => domainError,
            concurrencyError => concurrencyError);
    }

    private static List<string> GetServiceResourceReferences(UpdateDialogDto request)
    {
        var serviceResourceReferences = new List<string>();

        static bool IsExternalResource(string? resource)
        {
            return resource is not null && resource.StartsWith(Domain.Common.Constants.ServiceResourcePrefix, StringComparison.OrdinalIgnoreCase);
        }

        serviceResourceReferences.AddRange(request.ApiActions
            .Where(action => IsExternalResource(action.AuthorizationAttribute))
            .Select(action => action.AuthorizationAttribute!));
        serviceResourceReferences.AddRange(request.GuiActions
            .Where(action => IsExternalResource(action.AuthorizationAttribute))
            .Select(action => action.AuthorizationAttribute!));

        return serviceResourceReferences.Distinct().ToList();
    }

    private void ValidateTimeFields(DialogEntity dialog)
    {
        const string errorMessage = "Must be in future or current value.";

        if (!_db.MustWhenModified(dialog,
            propertyExpression: x => x.ExpiresAt,
            predicate: x => x > DateTimeOffset.UtcNow))
        {
            _domainContext.AddError(nameof(UpdateDialogCommand.Dto.ExpiresAt), errorMessage);
        }

        if (!_db.MustWhenModified(dialog,
            propertyExpression: x => x.DueAt,
            predicate: x => x > DateTimeOffset.UtcNow || x == null))
        {
            _domainContext.AddError(nameof(UpdateDialogCommand.Dto.DueAt), errorMessage + " (Or null)");
        }

        if (!_db.MustWhenModified(dialog,
            propertyExpression: x => x.VisibleFrom,
            predicate: x => x > DateTimeOffset.UtcNow))
        {
            _domainContext.AddError(nameof(UpdateDialogCommand.Dto.VisibleFrom), errorMessage);
        }
    }

    private async Task AppendActivity(DialogEntity dialog, UpdateDialogDto dto, CancellationToken cancellationToken)
    {
        var newDialogActivities = _mapper.Map<List<DialogActivity>>(dto.Activities);

        var existingIds = await _db.GetExistingIds(newDialogActivities, cancellationToken);
        if (existingIds.Count != 0)
        {
            _domainContext.AddError(
                nameof(UpdateDialogDto.Activities),
                $"Entity '{nameof(DialogActivity)}' with the following key(s) already exists: ({string.Join(", ", existingIds)}).");
        }

        dialog.Activities.AddRange(newDialogActivities);

        // Tell ef explicitly to add activities as new to the database.
        _db.DialogActivities.AddRange(newDialogActivities);
    }

    private void VerifyActivityTransmissionRelations(DialogEntity dialog)
    {
        var relatedTransmissionIds = dialog.Activities
            .Where(x => x.TransmissionId is not null)
            .Select(x => x.TransmissionId)
            .ToList();

        if (relatedTransmissionIds.Count == 0)
        {
            return;
        }

        var transmissionIds = dialog.Transmissions.Select(x => x.Id).ToList();

        var invalidTransmissionIds = relatedTransmissionIds
            .Where(id => !transmissionIds.Contains(id!.Value))
            .ToList();

        if (invalidTransmissionIds.Count != 0)
        {
            _domainContext.AddError(
                nameof(UpdateDialogDto.Activities),
                $"Invalid '{nameof(DialogActivity.TransmissionId)}, entity '{nameof(DialogTransmission)}'" +
                $" with the following key(s) does not exist: ({string.Join(", ", invalidTransmissionIds)}) in '{nameof(dialog.Transmissions)}'");
        }
    }

    private void VerifyActivityRelations(DialogEntity dialog)
    {
        var relatedActivityIds = dialog.Activities
            .Where(x => x.RelatedActivityId is not null)
            .Select(x => x.RelatedActivityId)
            .ToList();

        if (relatedActivityIds.Count == 0)
        {
            return;
        }

        var activityIds = dialog.Activities.Select(x => x.Id).ToList();

        var invalidRelatedActivityIds = relatedActivityIds
            .Where(id => !activityIds.Contains(id!.Value))
            .ToList();

        if (invalidRelatedActivityIds.Count != 0)
        {
            _domainContext.AddError(
                nameof(UpdateDialogDto.Activities),
                $"Invalid '{nameof(DialogActivity.RelatedActivityId)}, entity '{nameof(DialogActivity)}'" +
                $" with the following key(s) does not exist: ({string.Join(", ", invalidRelatedActivityIds)}) in '{nameof(dialog.Activities)}'");
        }
    }

    private void AppendTransmission(DialogEntity dialog, UpdateDialogDto dto)
    {
        var newDialogTransmissions = _mapper.Map<List<DialogTransmission>>(dto.Transmissions);

        var existingIds = dialog.Transmissions.Select(x => x.Id).ToList();

        existingIds = existingIds.Intersect(newDialogTransmissions.Select(x => x.Id)).ToList();
        if (existingIds.Count != 0)
        {
            _domainContext.AddError(
                nameof(UpdateDialogDto.Transmissions),
                $"Entity '{nameof(DialogTransmission)}' with the following key(s) already exists: ({string.Join(", ", existingIds)}).");
            return;
        }

        dialog.Transmissions.AddRange(newDialogTransmissions);

        // Tell ef explicitly to add transmissions as new to the database.
        _db.DialogTransmissions.AddRange(newDialogTransmissions);
    }

    private void VerifyTransmissionRelations(DialogEntity dialog)
    {
        var relatedTransmissionIds = dialog.Transmissions.Where(x => x.RelatedTransmissionId is not null).Select(x => x.RelatedTransmissionId).ToList();
        if (relatedTransmissionIds.Count == 0)
        {
            return;
        }

        var transmissionIds = dialog.Transmissions.Select(x => x.Id).ToList();

        var invalidRelatedTransmissionIds = relatedTransmissionIds
            .Where(id => !transmissionIds.Contains(id!.Value))
            .ToList();

        if (invalidRelatedTransmissionIds.Count != 0)
        {
            _domainContext.AddError(
                nameof(UpdateDialogDto.Transmissions),
                $"Invalid '{nameof(DialogTransmission.RelatedTransmissionId)}, entity '{nameof(DialogTransmission)}' with the following key(s) does not exist: ({string.Join(", ", invalidRelatedTransmissionIds)}).");
        }
    }

    private IEnumerable<DialogApiAction> CreateApiActions(IEnumerable<UpdateDialogDialogApiActionDto> creatables)
    {
        return creatables.Select(x =>
        {
            var apiAction = _mapper.Map<DialogApiAction>(x);
            apiAction.Endpoints = _mapper.Map<List<DialogApiActionEndpoint>>(x.Endpoints);
            return apiAction;
        });
    }

    private void UpdateApiActions(IEnumerable<UpdateSet<DialogApiAction, UpdateDialogDialogApiActionDto>> updateSets)
    {
        foreach (var (source, destination) in updateSets)
        {
            _mapper.Map(source, destination);

            destination.Endpoints
                .Merge(source.Endpoints,
                    destinationKeySelector: x => x.Id,
                    sourceKeySelector: x => x.Id,
                    create: _mapper.Map<List<DialogApiActionEndpoint>>,
                    update: _mapper.Update,
                    delete: DeleteDelegate.NoOp);
        }
    }

    private async Task<IEnumerable<DialogAttachment>> CreateAttachments(IEnumerable<UpdateDialogDialogAttachmentDto> creatables, CancellationToken cancellationToken)
    {
        var attachments = new List<DialogAttachment>();
        foreach (var atttachmentDto in creatables)
        {
            var attachment = _mapper.Map<DialogAttachment>(atttachmentDto);
            attachment.Urls = _mapper.Map<List<AttachmentUrl>>(atttachmentDto.Urls);
            attachments.Add(attachment);
        }

        var existingIds = await _db.GetExistingIds(attachments, cancellationToken);
        if (existingIds.Count != 0)
        {
            _domainContext.AddError(nameof(UpdateDialogDto.Attachments), $"Entity '{nameof(DialogAttachment)}' with the following key(s) already exists: ({string.Join(", ", existingIds)}).");
        }

        _db.DialogAttachments.AddRange(attachments);
        return attachments;
    }

    private Task UpdateAttachments(IEnumerable<UpdateSet<DialogAttachment, UpdateDialogDialogAttachmentDto>> updateSets, CancellationToken _)
    {
        foreach (var updateSet in updateSets)
        {
            _mapper.Map(updateSet.Source, updateSet.Destination);
            updateSet.Destination.Urls
                .Merge(updateSet.Source.Urls,
                    destinationKeySelector: x => x.Id,
                    sourceKeySelector: x => x.Id,
                    create: _mapper.Map<List<AttachmentUrl>>,
                    update: _mapper.Update,
                    delete: DeleteDelegate.NoOp);
        }

        return Task.CompletedTask;
    }
}
