using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Common.Interfaces;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Domain.Dialogues;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Attachments;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Commands.Update;

public class UpdateDialogueCommand : IRequest 
{
    public Guid Id { get; set; }
    public UpdateDialogueDto Dto { get; set; } = null!;
}

internal sealed class UpdateDialogueCommandHandler : AsyncRequestHandler<UpdateDialogueCommand>
{
    private readonly IDialogueDbContext _db;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDialogueCommandHandler(IDialogueDbContext db, IMapper mapper, IUnitOfWork unitOfWork)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    protected override async Task Handle(UpdateDialogueCommand request, CancellationToken cancellationToken)
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
            throw new Exception();
        }

        // Update primitive properties
        _mapper.Map(request.Dto, dialogue);

        // History is append only, therefore no need to load history
        // TODO: What if consumer sends existing history id? 
        dialogue.History.AddRange(await CreateHistory(request.Dto.History, cancellationToken));

        dialogue.Body.Localizations = await dialogue.Body.Localizations
            .MergeAsync(request.Dto.Body,
                destinationKeySelector: x => x.CultureCode,
                sourceKeySelector: x => x.CultureCode,
                create: CreateLocalization,
                update: UpdateLocalization,
                delete: DeleteLocalization,
                cancellationToken: cancellationToken);

        dialogue.Title.Localizations = await dialogue.Title.Localizations
            .MergeAsync(request.Dto.Title,
                destinationKeySelector: x => x.CultureCode,
                sourceKeySelector: x => x.CultureCode,
                create: CreateLocalization,
                update: UpdateLocalization,
                delete: DeleteLocalization,
                cancellationToken: cancellationToken);

        dialogue.SenderName.Localizations = await dialogue.SenderName.Localizations
            .MergeAsync(request.Dto.SenderName,
                destinationKeySelector: x => x.CultureCode,
                sourceKeySelector: x => x.CultureCode,
                create: CreateLocalization,
                update: UpdateLocalization,
                delete: DeleteLocalization,
                cancellationToken: cancellationToken);

        dialogue.SearchTitle.Localizations = await dialogue.SearchTitle.Localizations
            .MergeAsync(request.Dto.SearchTitle,
                destinationKeySelector: x => x.CultureCode,
                sourceKeySelector: x => x.CultureCode,
                create: CreateLocalization,
                update: UpdateLocalization,
                delete: DeleteLocalization,
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
    }

    private Task<IEnumerable<Localization>> CreateLocalization(IEnumerable<LocalizationDto> creatables, CancellationToken cancellationToken)
    {
        var result = _mapper.Map<List<Localization>>(creatables);
        return Task.FromResult<IEnumerable<Localization>>(result);
    }

    private Task UpdateLocalization(IEnumerable<UpdateSet<Localization, LocalizationDto>> updateSets, CancellationToken cancellationToken)
    {
        foreach (var updateSet in updateSets)
        {
            _mapper.Map(updateSet.Source, updateSet.Destination);
        }

        return Task.CompletedTask;
    }

    private Task DeleteLocalization(IEnumerable<Localization> deletables, CancellationToken cancellationToken)
    {
        _db.Localizations.RemoveRange(deletables);
        return Task.CompletedTask;
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

    private Task<IEnumerable<DialogueActivity>> CreateHistory(List<UpdateDialogueDialogueActivityDto> creatables, CancellationToken cancellationToken)
    {
        var result = _mapper.Map<List<DialogueActivity>>(creatables);
        return Task.FromResult<IEnumerable<DialogueActivity>>(result);
    }

    private Task<IEnumerable<DialogueApiAction>> CreateApiActions(IEnumerable<UpdateDialogueDialogueApiActionDto> creatables, CancellationToken cancellationToken)
    {
        var result = _mapper.Map<List<DialogueApiAction>>(creatables);
        return Task.FromResult<IEnumerable<DialogueApiAction>>(result);
    }

    private Task UpdateApiActions(IEnumerable<UpdateSet<DialogueApiAction, UpdateDialogueDialogueApiActionDto>> updateSets, CancellationToken cancellationToken)
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

    private async Task UpdateGuiActions(IEnumerable<UpdateSet<DialogueGuiAction, UpdateDialogueDialogueGuiActionDto>> updateSets, CancellationToken cancellationToken)
    {
        foreach (var updateSet in updateSets)
        {
            _mapper.Map(updateSet.Source, updateSet.Destination);

            updateSet.Destination.Title.Localizations = await updateSet.Destination.Title.Localizations
                .MergeAsync(updateSet.Source.Title,
                    destinationKeySelector: x => x.CultureCode,
                    sourceKeySelector: x => x.CultureCode,
                    create: CreateLocalization,
                    update: UpdateLocalization,
                    delete: DeleteLocalization,
                    cancellationToken: cancellationToken);
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

    private async Task UpdateAttachments(IEnumerable<UpdateSet<DialogueAttachement, UpdateDialogueDialogueAttachmentDto>> updateSets, CancellationToken cancellationToken)
    {
        foreach (var updateSet in updateSets)
        {
            _mapper.Map(updateSet.Source, updateSet.Destination);

            updateSet.Destination.DisplayName.Localizations = await updateSet.Destination.DisplayName.Localizations
                .MergeAsync(updateSet.Source.DisplayName,
                    destinationKeySelector: x => x.CultureCode,
                    sourceKeySelector: x => x.CultureCode,
                    create: CreateLocalization,
                    update: UpdateLocalization,
                    delete: DeleteLocalization,
                    cancellationToken: cancellationToken);
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
