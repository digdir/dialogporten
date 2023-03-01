using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Domain.Dialogues;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Attachments;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Commands.Update;

public sealed class UpdateDialogueCommand : IRequest<OneOf<Success, EntityNotFound, ValidationError>>
{
    public Guid Id { get; set; }
    public UpdateDialogueDto Dto { get; set; } = null!;
}

internal sealed class UpdateDialogueCommandHandler : IRequestHandler<UpdateDialogueCommand, OneOf<Success, EntityNotFound, ValidationError>>
{
    private readonly IDialogueDbContext _db;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILocalizationService _localizationService;

    public UpdateDialogueCommandHandler(IDialogueDbContext db, IMapper mapper, IUnitOfWork unitOfWork, ILocalizationService localizationService)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
    }

    public async Task<OneOf<Success, EntityNotFound, ValidationError>> Handle(UpdateDialogueCommand request, CancellationToken cancellationToken)
    {
        var dialogue = await _db.Dialogues
            .Include(x => x.Body.Localizations)
            .Include(x => x.Title.Localizations)
            .Include(x => x.SenderName.Localizations)
            .Include(x => x.SearchTitle.Localizations)
            .Include(x => x.History)
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

        // Update primitive properties
        _mapper.Map(request.Dto, dialogue);

        await _localizationService.Merge(dialogue.Body, request.Dto.Body, cancellationToken);
        await _localizationService.Merge(dialogue.Title, request.Dto.Title, cancellationToken);
        await _localizationService.Merge(dialogue.SenderName, request.Dto.SenderName, cancellationToken);
        await _localizationService.Merge(dialogue.SearchTitle, request.Dto.SearchTitle, cancellationToken);

        dialogue.History = await dialogue.History
            .MergeAsync(request.Dto.History,
                destinationKeySelector: x => x.Id,
                sourceKeySelector: x => x.Id,
                create: CreateHistory,
                update: UpdateHistory,
                cancellationToken: cancellationToken);

        dialogue.Attachments = await dialogue.Attachments
            .MergeAsync(request.Dto.Attachments,
                destinationKeySelector: x => x.Id,
                sourceKeySelector: x => x.Id,
                create: CreateAttachments,
                update: UpdateAttachments,
                delete: DeleteAttachments,
                cancellationToken: cancellationToken);

        dialogue.GuiActions = await dialogue.GuiActions
            .MergeAsync(request.Dto.GuiActions,
                destinationKeySelector: x => x.Id,
                sourceKeySelector: x => x.Id,
                create: CreateGuiActions,
                update: UpdateGuiActions,
                delete: DeleteGuiActions,
                cancellationToken: cancellationToken);

        dialogue.ApiActions = await dialogue.ApiActions
            .MergeAsync(request.Dto.ApiActions,
                destinationKeySelector: x => x.Id,
                sourceKeySelector: x => x.Id,
                create: CreateApiActions,
                update: UpdateApiActions,
                delete: DeleteApiActions,
                cancellationToken: cancellationToken);

        dialogue.TokenScopes = await dialogue.TokenScopes
            .MergeAsync(request.Dto.TokenScopes,
                destinationKeySelector: x => x.Value,
                sourceKeySelector: x => x.Value,
                create: CreateTokenScope,
                delete: DeleteTokenScope,
                cancellationToken: cancellationToken);

        // TODO: Publish event

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new Success();
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

    private Task<IEnumerable<DialogueActivity>> CreateHistory(IEnumerable<UpdateDialogueDialogueActivityDto> creatables, CancellationToken cancellationToken)
    {
        var result = _mapper.Map<List<DialogueActivity>>(creatables);
        return Task.FromResult<IEnumerable<DialogueActivity>>(result);
    }

    private Task UpdateHistory(IEnumerable<IUpdateSet<DialogueActivity, UpdateDialogueDialogueActivityDto>> updateSets, CancellationToken cancellationToken)
    {
        if (updateSets.Any())
        {
            // TODO: DomainError?
        }
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
