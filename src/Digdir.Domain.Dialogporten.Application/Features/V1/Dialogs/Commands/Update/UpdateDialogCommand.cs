using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using Digdir.Library.Entity.Abstractions.Features.Identifiable;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Commands.Update;

public sealed class UpdateDialogCommand : IRequest<OneOf<Success, EntityNotFound, EntityExists, ValidationError, DomainError>>
{
    public Guid Id { get; set; }
    public UpdateDialogDto Dto { get; set; } = null!;
}

internal sealed class UpdateDialogCommandHandler : IRequestHandler<UpdateDialogCommand, OneOf<Success, EntityNotFound, EntityExists, ValidationError, DomainError>>
{
    private readonly IDialogDbContext _db;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILocalizationService _localizationService;
    private readonly IDomainEventPublisher _eventPublisher;
    private readonly IDomainContext _domainContext;

    public UpdateDialogCommandHandler(
        IDialogDbContext db,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ILocalizationService localizationService,
        IDomainEventPublisher eventPublisher,
        IDomainContext domainContext)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _domainContext = domainContext ?? throw new ArgumentNullException(nameof(domainContext));
    }

    public async Task<OneOf<Success, EntityNotFound, EntityExists, ValidationError, DomainError>> Handle(UpdateDialogCommand request, CancellationToken cancellationToken)
    {
        var dialog = await _db.Dialogs
            .Include(x => x.Body.Localizations)
            .Include(x => x.Title.Localizations)
            .Include(x => x.SenderName.Localizations)
            .Include(x => x.SearchTitle.Localizations)
            .Include(x => x.Elements)
                .ThenInclude(x => x.DisplayName.Localizations)
            .Include(x => x.Elements)
                .ThenInclude(x => x.Urls)
            .Include(x => x.GuiActions)
                .ThenInclude(x => x.Title.Localizations)
            .Include(x => x.ApiActions)
                .ThenInclude(x => x.Endpoints)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (dialog is null)
        {
            return new EntityNotFound<DialogEntity>(request.Id);
        }

        // Update primitive properties
        _mapper.Map(request.Dto, dialog);
        ValidateTimeFields(dialog);
        await AppendActivity(dialog, request.Dto, cancellationToken);

        await _localizationService.Merge(dialog.Body, request.Dto.Body, cancellationToken);
        await _localizationService.Merge(dialog.Title, request.Dto.Title, cancellationToken);
        await _localizationService.Merge(dialog.SenderName, request.Dto.SenderName, cancellationToken);
        await _localizationService.Merge(dialog.SearchTitle, request.Dto.SearchTitle, cancellationToken);

        dialog.Elements = await dialog.Elements
            .MergeAsync(request.Dto.Elements,
                destinationKeySelector: x => x.Id,
                sourceKeySelector: x => x.Id,
                create: CreateElements,
                update: UpdateElements,
                delete: DeleteElements,
                cancellationToken: cancellationToken);

        dialog.GuiActions = await dialog.GuiActions
            .MergeAsync(request.Dto.GuiActions,
                destinationKeySelector: x => x.Id,
                sourceKeySelector: x => x.Id,
                create: CreateGuiActions,
                update: UpdateGuiActions,
                delete: DeleteGuiActions,
                cancellationToken: cancellationToken);

        dialog.ApiActions = await dialog.ApiActions
            .MergeAsync(request.Dto.ApiActions,
                destinationKeySelector: x => x.Id,
                sourceKeySelector: x => x.Id,
                create: CreateApiActions,
                update: UpdateApiActions,
                delete: DeleteApiActions,
                cancellationToken: cancellationToken);

        _eventPublisher.Publish(new DialogUpdatedDomainEvent(dialog.Id));
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new Success();
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
            predicate: x => x > DateTimeOffset.UtcNow))
        {
            _domainContext.AddError(nameof(UpdateDialogCommand.Dto.DueAt), errorMessage);
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
        var newDialogActivities = dto.Activities
            .Select(_mapper.Map<DialogActivity>)
            .ToList();

        var existingIds = await _db.GetExistingIds(newDialogActivities, cancellationToken);
        if (existingIds.Any())
        {
            // TODO: Should this be a EntityExists?
            _domainContext.AddError(nameof(UpdateDialogDto.Activities), $"Entity '{nameof(DialogActivity)}' with the following key(s) allready exists: ({string.Join(", ", existingIds)}).");
        }

        _eventPublisher.Publish(newDialogActivities.Select(x => new DialogActivityCreatedDomainEvent(dialog.Id, x.CreateId())));
        dialog.Activities.AddRange(newDialogActivities);

        // Tell ef explicitly to add the new activities to the database.
        _db.DialogActivities.AddRange(newDialogActivities);
    }

    private async Task<IEnumerable<DialogApiAction>> CreateApiActions(IEnumerable<UpdateDialogDialogApiActionDto> creatables, CancellationToken cancellationToken)
    {
        var apiActions = new List<DialogApiAction>();
        foreach (var apiActionDto in creatables)
        {
            var apiAction = _mapper.Map<DialogApiAction>(apiActionDto);
            apiAction.Endpoints = (await CreateApiActionEndpoints(apiActionDto.Endpoints, cancellationToken)).ToList();
            apiActions.Add(apiAction);
        }

        return apiActions;
    }

    private async Task UpdateApiActions(IEnumerable<IUpdateSet<DialogApiAction, UpdateDialogDialogApiActionDto>> updateSets, CancellationToken cancellationToken)
    {
        foreach (var updateSet in updateSets)
        {
            _mapper.Map(updateSet.Source, updateSet.Destination);

            updateSet.Destination.Endpoints = await updateSet.Destination.Endpoints
                .MergeAsync(updateSet.Source.Endpoints,
                    destinationKeySelector: x => x.Id,
                    sourceKeySelector: x => x.Id,
                    create: CreateApiActionEndpoints,
                    update: UpdateApiActionEndpoints,
                    delete: DeleteApiActionEndpoints,
                    cancellationToken: cancellationToken);
        }
    }

    private Task DeleteApiActions(IEnumerable<DialogApiAction> deletables, CancellationToken cancellationToken)
    {
        _db.DialogApiActions.RemoveRange(deletables);
        return Task.CompletedTask;
    }

    private Task<IEnumerable<DialogApiActionEndpoint>> CreateApiActionEndpoints(IEnumerable<UpdateDialogDialogApiActionEndpointDto> creatables, CancellationToken cancellationToken)
    {
        return Task.FromResult(creatables.Select(_mapper.Map<DialogApiActionEndpoint>));
    }

    private Task UpdateApiActionEndpoints(IEnumerable<IUpdateSet<DialogApiActionEndpoint, UpdateDialogDialogApiActionEndpointDto>> updateSets, CancellationToken cancellationToken)
    {
        foreach (var updateSet in updateSets)
        {
            _mapper.Map(updateSet.Source, updateSet.Destination);
        }
        return Task.CompletedTask;
    }

    private Task DeleteApiActionEndpoints(IEnumerable<DialogApiActionEndpoint> deletables, CancellationToken cancellationToken)
    {
        _db.DialogApiActionEndpoints.RemoveRange(deletables);
        return Task.CompletedTask;
    }

    private Task<IEnumerable<DialogGuiAction>> CreateGuiActions(IEnumerable<UpdateDialogDialogGuiActionDto> creatables, CancellationToken cancellationToken)
    {
        return Task.FromResult(creatables.Select(x =>
        {
            var guiAction = _mapper.Map<DialogGuiAction>(x);
            guiAction.Title = _mapper.Map<LocalizationSet>(x.Title);
            return guiAction;
        }));
    }

    private async Task UpdateGuiActions(IEnumerable<IUpdateSet<DialogGuiAction, UpdateDialogDialogGuiActionDto>> updateSets, CancellationToken cancellationToken)
    {
        foreach (var updateSet in updateSets)
        {
            _mapper.Map(updateSet.Source, updateSet.Destination);
            await _localizationService.Merge(updateSet.Destination.Title, updateSet.Source.Title, cancellationToken);
        }
    }

    private Task DeleteGuiActions(IEnumerable<DialogGuiAction> deletables, CancellationToken cancellationToken)
    {
        deletables = deletables is List<DialogGuiAction> ? deletables : deletables.ToList();
        _db.DialogGuiActions.RemoveRange(deletables);
        _db.LocalizationSets.RemoveRange(deletables.Select(x => x.Title));
        return Task.CompletedTask;
    }

    private async Task<IEnumerable<DialogElement>> CreateElements(IEnumerable<UpdateDialogDialogElementDto> creatables, CancellationToken cancellationToken)
    {
        var elements = new List<DialogElement>();
        foreach (var elementDto in creatables)
        {
            var element = _mapper.Map<DialogElement>(elementDto);
            element.DisplayName = _mapper.Map<LocalizationSet>(elementDto.DisplayName);
            element.Urls = (await CreateElementUrls(elementDto.Urls, cancellationToken)).ToList();
            elements.Add(element);
        }

        var existingIds = await _db.GetExistingIds(elements, cancellationToken);
        if (existingIds.Any())
        {
            // TODO: Should this be a EntityExists?
            _domainContext.AddError(nameof(UpdateDialogDto.Elements), $"Entity '{nameof(DialogElement)}' with the following key(s) allready exists: ({string.Join(", ", existingIds)}).");
        }

        _db.DialogElements.AddRange(elements);
        return elements;
    }

    private async Task UpdateElements(IEnumerable<IUpdateSet<DialogElement, UpdateDialogDialogElementDto>> updateSets, CancellationToken cancellationToken)
    {
        foreach (var updateSet in updateSets)
        {
            _mapper.Map(updateSet.Source, updateSet.Destination);
            await _localizationService.Merge(updateSet.Destination.DisplayName, updateSet.Source.DisplayName, cancellationToken);

            updateSet.Destination.Urls = await updateSet.Destination.Urls
                .MergeAsync(updateSet.Source.Urls,
                    destinationKeySelector: x => x.Id,
                    sourceKeySelector: x => x.Id,
                    create: CreateElementUrls,
                    update: UpdateElementUrls,
                    delete: DeleteElementUrls,
                    cancellationToken: cancellationToken);
        }
    }

    private async Task DeleteElements(IEnumerable<DialogElement> deletables, CancellationToken cancellationToken)
    {
        deletables = deletables is List<DialogGuiAction> ? deletables : deletables.ToList();
        _db.DialogElements.RemoveRange(deletables);
        _db.LocalizationSets.RemoveRange(deletables.Select(x => x.DisplayName));
        await DeleteElementUrls(deletables.SelectMany(x => x.Urls), cancellationToken);
    }

    private Task<IEnumerable<DialogElementUrl>> CreateElementUrls(IEnumerable<UpdateDialogDialogElementUrlDto> creatables, CancellationToken cancellationToken)
    {
        return Task.FromResult(creatables.Select(_mapper.Map<DialogElementUrl>));
    }

    private Task UpdateElementUrls(IEnumerable<IUpdateSet<DialogElementUrl, UpdateDialogDialogElementUrlDto>> updateSets, CancellationToken cancellationToken)
    {
        foreach (var updateSet in updateSets)
        {
            _mapper.Map(updateSet.Source, updateSet.Destination);
        }

        return Task.CompletedTask;
    }

    private Task DeleteElementUrls(IEnumerable<DialogElementUrl> deletables, CancellationToken cancellationToken)
    {
        _db.DialogElementUrls.RemoveRange(deletables);
        return Task.CompletedTask;
    }
}
