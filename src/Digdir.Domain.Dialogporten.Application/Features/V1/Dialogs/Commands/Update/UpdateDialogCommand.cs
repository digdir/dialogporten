using AutoMapper;
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
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Commands.Update;

public sealed class UpdateDialogCommand : IRequest<OneOf<Success, EntityNotFound, EntityExists, ValidationError>>
{
    public Guid Id { get; set; }
    public UpdateDialogDto Dto { get; set; } = null!;
}

internal sealed class UpdateDialogCommandHandler : IRequestHandler<UpdateDialogCommand, OneOf<Success, EntityNotFound, EntityExists, ValidationError>>
{
    private readonly IDialogDbContext _db;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILocalizationService _localizationService;
    private readonly IDomainEventPublisher _eventPublisher;

    public UpdateDialogCommandHandler(
        IDialogDbContext db,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ILocalizationService localizationService,
        IDomainEventPublisher eventPublisher)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
    }

    public async Task<OneOf<Success, EntityNotFound, EntityExists, ValidationError>> Handle(UpdateDialogCommand request, CancellationToken cancellationToken)
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

        var dto = request.Dto;
        var existingActivityIds = await GetExistingActivityIds(dto.Activities, cancellationToken);
        if (existingActivityIds.Any())
        {
            return new EntityExists<DialogActivity>(existingActivityIds);
        }

        // Update primitive properties
        _mapper.Map(dto, dialog);

        // Append activity
        AppendActivity(dialog, dto);

        await _localizationService.Merge(dialog.Body, dto.Body, cancellationToken);
        await _localizationService.Merge(dialog.Title, dto.Title, cancellationToken);
        await _localizationService.Merge(dialog.SenderName, dto.SenderName, cancellationToken);
        await _localizationService.Merge(dialog.SearchTitle, dto.SearchTitle, cancellationToken);

        dialog.Elements = await dialog.Elements
            .MergeAsync(dto.Elements,
                destinationKeySelector: x => x.Id,
                sourceKeySelector: x => x.Id,
                create: CreateElements,
                update: UpdateElements,
                delete: DeleteElements,
                cancellationToken: cancellationToken);

        dialog.GuiActions = await dialog.GuiActions
            .MergeAsync(dto.GuiActions,
                destinationKeySelector: x => x.Id,
                sourceKeySelector: x => x.Id,
                create: CreateGuiActions,
                update: UpdateGuiActions,
                delete: DeleteGuiActions,
                cancellationToken: cancellationToken);

        dialog.ApiActions = await dialog.ApiActions
            .MergeAsync(dto.ApiActions,
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

    private void AppendActivity(DialogEntity dialog, UpdateDialogDto dto)
    {
        var newDialogActivities = dto.Activities
            .Select(_mapper.Map<DialogActivity>)
            .ToList();
        _eventPublisher.Publish(newDialogActivities.Select(x => new DialogActivityCreatedDomainEvent(dialog.Id, x.CreateId())));
        dialog.Activities.AddRange(newDialogActivities);

        // Tell ef explicitly to add the new activities to the database.
        _db.DialogActivities.AddRange(newDialogActivities);
    }

    private async Task<IEnumerable<Guid>> GetExistingActivityIds(
        List<UpdateDialogDialogActivityDto> activityDtos,
        CancellationToken cancellationToken)
    {
        var activityDtoIds = activityDtos
            .Select(x => x.Id)
            .Where(x => x.HasValue)
            .ToList();

        if (!activityDtoIds.Any())
        {
            return Enumerable.Empty<Guid>();
        }

        return await _db.DialogActivities
            .Select(x => x.Id)
            .Where(x => activityDtoIds.Contains(x))
            .ToListAsync(cancellationToken);
    }

    private async Task<IEnumerable<DialogApiAction>> CreateApiActions(IEnumerable<UpdateDialogDialogApiActionDto> creatables, CancellationToken cancellationToken)
    {
        var dtos = creatables.ToList();
        //var invalidIds = dtos
        //    .Where(x => x.Id.HasValue)
        //    .Select(x => x.Id!.Value)
        //    .ToList();

        //if (invalidIds.Any())
        //{
        //    // TODO: Create a domain error as user should not be able to set the id of a new item
        //}

        var apiActions = new List<DialogApiAction>();
        foreach (var apiActionDto in dtos)
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
        var dtos = creatables.ToList();
        //var invalidIds = dtos
        //    .Where(x => x.Id.HasValue)
        //    .Select(x => x.Id!.Value)
        //    .ToList();

        //if (invalidIds.Any())
        //{
        //    // TODO: Create a domain error as user should not be able to set the id of a new item
        //}

        return Task.FromResult(dtos.Select(_mapper.Map<DialogApiActionEndpoint>));
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
        var dtos = creatables.ToList();
        //var invalidIds = dtos
        //    .Where(x => x.Id.HasValue)
        //    .Select(x => x.Id!.Value)
        //    .ToList();

        //if (invalidIds.Any())
        //{
        //    // TODO: Create a domain error as user should not be able to set the id of a new item
        //}

        return Task.FromResult(dtos.Select(x =>
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
        var dtos = creatables.ToList();
        //var invalidIds = dtos
        //    .Where(x => x.Id.HasValue)
        //    .Select(x => x.Id!.Value)
        //    .ToList();

        //if (invalidIds.Any())
        //{
        //    // TODO: Create a domain error as user should not be able to set the id of a new item
        //}

        return Task.FromResult(dtos.Select(_mapper.Map<DialogElementUrl>));
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
