using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Domain.Dialogues;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities.Attachments;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities.TokenScopes;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using FluentValidation;
using Json.Patch;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;
using System.Text.Json;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Commands.Update;

public sealed class UpdateDialogueCommand : IRequest<OneOf<Success, EntityNotFound, EntityExists, ValidationError>>
{
    public Guid Id { get; set; }
    public OneOf<UpdateDialogueDto, JsonPatch> Dto { get; set; }
}

internal sealed class UpdateDialogueCommandHandler : IRequestHandler<UpdateDialogueCommand, OneOf<Success, EntityNotFound, EntityExists, ValidationError>>
{
    private readonly IDialogueDbContext _db;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILocalizationService _localizationService;
    private readonly IEnumerable<IValidator<UpdateDialogueDto>> _validators;
    private readonly IDomainEventPublisher _eventPublisher;

    public UpdateDialogueCommandHandler(
        IDialogueDbContext db,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ILocalizationService localizationService,
        IEnumerable<IValidator<UpdateDialogueDto>> validators,
        IDomainEventPublisher eventPublisher)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _validators = validators ?? throw new ArgumentNullException(nameof(validators));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
    }

    public async Task<OneOf<Success, EntityNotFound, EntityExists, ValidationError>> Handle(UpdateDialogueCommand request, CancellationToken cancellationToken)
    {
        var dialogue = await _db.Dialogues
            .Include(x => x.Body.Localizations)
            .Include(x => x.Title.Localizations)
            .Include(x => x.SenderName.Localizations)
            .Include(x => x.SearchTitle.Localizations)
            .Include(x => x.Attachments)
                .ThenInclude(attachment => attachment.DisplayName.Localizations)
            .Include(x => x.GuiActions)
                .ThenInclude(guiAction => guiAction.Title.Localizations)
            .Include(x => x.ApiActions)
            .Include(x => x.TokenScopes)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (dialogue is null)
        {
            return new EntityNotFound<DialogueEntity>(request.Id);
        }

        var dtoOrValidationError = request.Dto.Match(
            dto => dto, 
            jsonPatch => CreateDto(dialogue, jsonPatch));
        if (dtoOrValidationError.TryPickT1(out var validationError, out var dto))
        {
            return validationError;
        }

        var existingHistoryIds = await GetExistingHistoryIds(dto.History, cancellationToken);
        if (existingHistoryIds.Any())
        {
            return new EntityExists<DialogueActivity>(existingHistoryIds);
        }

        // Update primitive properties
        _mapper.Map(dto, dialogue);

        // Append history
        dialogue.History.AddRange(dto.History.Select(_mapper.Map<DialogueActivity>));

        await _localizationService.Merge(dialogue.Body, dto.Body, cancellationToken);
        await _localizationService.Merge(dialogue.Title, dto.Title, cancellationToken);
        await _localizationService.Merge(dialogue.SenderName, dto.SenderName, cancellationToken);
        await _localizationService.Merge(dialogue.SearchTitle, dto.SearchTitle, cancellationToken);

        dialogue.Attachments = await dialogue.Attachments
            .MergeAsync(dto.Attachments,
                destinationKeySelector: x => x.Id,
                sourceKeySelector: x => x.Id,
                create: CreateAttachments,
                update: UpdateAttachments,
                delete: DeleteAttachments,
                cancellationToken: cancellationToken);

        dialogue.GuiActions = await dialogue.GuiActions
            .MergeAsync(dto.GuiActions,
                destinationKeySelector: x => x.Id,
                sourceKeySelector: x => x.Id,
                create: CreateGuiActions,
                update: UpdateGuiActions,
                delete: DeleteGuiActions,
                cancellationToken: cancellationToken);

        dialogue.ApiActions = await dialogue.ApiActions
            .MergeAsync(dto.ApiActions,
                destinationKeySelector: x => x.Id,
                sourceKeySelector: x => x.Id,
                create: CreateApiActions,
                update: UpdateApiActions,
                delete: DeleteApiActions,
                cancellationToken: cancellationToken);

        dialogue.TokenScopes = await dialogue.TokenScopes
            .MergeAsync(dto.TokenScopes,
                destinationKeySelector: x => x.Value,
                sourceKeySelector: x => x.Value,
                create: CreateTokenScope,
                delete: DeleteTokenScope,
                cancellationToken: cancellationToken);

        // TODO: Publish event
        _eventPublisher.Publish(new DialogueUpdatedDomainEvent(dialogue.Id));
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new Success();
    }

    private OneOf<UpdateDialogueDto, ValidationError> CreateDto(DialogueEntity dialogue, JsonPatch jsonPatch)
    {
        var originalAsDto = _mapper.Map<UpdateDialogueDto>(dialogue);
        var modifiedAsDto = jsonPatch.Apply(originalAsDto, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (modifiedAsDto is null)
        {
            // TODO: Better exception.
            throw new Exception();
        }

        var context = new ValidationContext<UpdateDialogueDto>(modifiedAsDto);
        var failures = _validators
            .Select(x => x.Validate(context))
            .SelectMany(x => x.Errors)
            .Where(x => x is not null)
            .ToList();

        return failures.Any()
            ? new ValidationError(failures)
            : modifiedAsDto;
    }

    private async Task<IEnumerable<Guid>> GetExistingHistoryIds(
        List<UpdateDialogueDialogueActivityDto> historyDtos,
        CancellationToken cancellationToken)
    {
        var historyDtoIds = historyDtos
            .Select(x => x.Id)
            .Where(x => x.HasValue)
            .ToList();

        if (!historyDtoIds.Any())
        {
            return Enumerable.Empty<Guid>();
        }

        return await _db.DialogueActivities
            .Select(x => x.Id)
            .Where(x => historyDtoIds.Contains(x))
            .ToListAsync(cancellationToken);
    }

    private Task<IEnumerable<DialogueTokenScope>> CreateTokenScope(IEnumerable<UpdateDialogueDialogueTokenScopeDto> creatables, CancellationToken cancellationToken)
    {
        var result = _mapper.Map<List<DialogueTokenScope>>(creatables);
        return Task.FromResult<IEnumerable<DialogueTokenScope>>(result);
    }

    private Task DeleteTokenScope(IEnumerable<DialogueTokenScope> deletables, CancellationToken cancellationToken)
    {
        _db.DialogueTokenScopes.RemoveRange(deletables);
        return Task.CompletedTask;
    }

    private Task<IEnumerable<DialogueApiAction>> CreateApiActions(IEnumerable<UpdateDialogueDialogueApiActionDto> creatables, CancellationToken cancellationToken)
    {
        var result = _mapper.Map<List<DialogueApiAction>>(creatables);
        return Task.FromResult<IEnumerable<DialogueApiAction>>(result);
    }

    private Task UpdateApiActions(IEnumerable<IUpdateSet<DialogueApiAction, UpdateDialogueDialogueApiActionDto>> updateSets, CancellationToken cancellationToken)
    {
        foreach (var updateSet in updateSets)
        {
            _mapper.Map(updateSet.Source, updateSet.Destination);
        }

        return Task.CompletedTask;
    }

    private Task DeleteApiActions(IEnumerable<DialogueApiAction> deletables, CancellationToken cancellationToken)
    {
        _db.DialogueApiActions.RemoveRange(deletables);
        return Task.CompletedTask;
    }

    private Task<IEnumerable<DialogueGuiAction>> CreateGuiActions(IEnumerable<UpdateDialogueDialogueGuiActionDto> guiActionDtos, CancellationToken cancellationToken)
    {
        return Task.FromResult(guiActionDtos.Select(x =>
        {
            var guiAction = _mapper.Map<DialogueGuiAction>(x);
            guiAction.Title = _mapper.Map<LocalizationSet>(x.Title);
            return guiAction;
        }));
    }

    private async Task UpdateGuiActions(IEnumerable<IUpdateSet<DialogueGuiAction, UpdateDialogueDialogueGuiActionDto>> updateSets, CancellationToken cancellationToken)
    {
        foreach (var updateSet in updateSets)
        {
            _mapper.Map(updateSet.Source, updateSet.Destination);
            await _localizationService.Merge(updateSet.Destination.Title, updateSet.Source.Title, cancellationToken);
        }
    }

    private Task DeleteGuiActions(IEnumerable<DialogueGuiAction> deletables, CancellationToken cancellationToken)
    {
        deletables = deletables is List<DialogueGuiAction> ? deletables : deletables.ToList();
        _db.LocalizationSets.RemoveRange(deletables.Select(x => x.Title));
        _db.DialogueGuiActions.RemoveRange(deletables);
        return Task.CompletedTask;
    }

    private Task<IEnumerable<DialogueAttachement>> CreateAttachments(IEnumerable<UpdateDialogueDialogueAttachmentDto> creatables, CancellationToken cancellationToken)
    {
        return Task.FromResult(creatables.Select(dto =>
        {
            var attachment = _mapper.Map<DialogueAttachement>(dto);
            attachment.DisplayName = _mapper.Map<LocalizationSet>(dto.DisplayName);
            return attachment;
        }));
    }

    private async Task UpdateAttachments(IEnumerable<IUpdateSet<DialogueAttachement, UpdateDialogueDialogueAttachmentDto>> updateSets, CancellationToken cancellationToken)
    {
        foreach (var updateSet in updateSets)
        {
            _mapper.Map(updateSet.Source, updateSet.Destination);
            await _localizationService.Merge(updateSet.Destination.DisplayName, updateSet.Source.DisplayName, cancellationToken);
        }
    }

    private Task DeleteAttachments(IEnumerable<DialogueAttachement> deletables, CancellationToken cancellationToken)
    {
        deletables = deletables is List<DialogueGuiAction> ? deletables : deletables.ToList();
        _db.LocalizationSets.RemoveRange(deletables.Select(x => x.DisplayName));
        _db.DialogueAttachements.RemoveRange(deletables);
        return Task.CompletedTask;
    }
}
