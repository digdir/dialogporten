using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.Extensions.Enumerable;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Update;

public sealed class UpdateDialogCommand : IRequest<UpdateDialogResult>
{
    public Guid Id { get; set; }
    public Guid? ETag { get; set; }
    public UpdateDialogDto Dto { get; set; } = null!;
}

[GenerateOneOf]
public partial class UpdateDialogResult : OneOfBase<Success, EntityNotFound, ValidationError, DomainError, ConcurrencyError> { }

internal sealed class UpdateDialogCommandHandler : IRequestHandler<UpdateDialogCommand, UpdateDialogResult>
{
    private readonly IDialogDbContext _db;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILocalizationService _localizationService;
    private readonly IDomainContext _domainContext;
    private readonly IUserService _userService;

    public UpdateDialogCommandHandler(
        IDialogDbContext db,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ILocalizationService localizationService,
        IDomainContext domainContext,
        IUserService userService)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _domainContext = domainContext ?? throw new ArgumentNullException(nameof(domainContext));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public async Task<UpdateDialogResult> Handle(UpdateDialogCommand request, CancellationToken cancellationToken)
    {
        var resourceIds = await _userService.GetCurrentUserResourceIds(cancellationToken);
        
        var dialog = await _db.Dialogs
            .Include(x => x.Body!.Localizations)
            .Include(x => x.Title!.Localizations)
            .Include(x => x.SenderName!.Localizations)
            .Include(x => x.SearchTags)
            .Include(x => x.Elements)
                .ThenInclude(x => x.DisplayName!.Localizations)
            .Include(x => x.Elements)
                .ThenInclude(x => x.Urls)
            .Include(x => x.GuiActions)
                .ThenInclude(x => x.Title!.Localizations)
            .Include(x => x.ApiActions)
                .ThenInclude(x => x.Endpoints)
            .Where(x => resourceIds.Contains(x.ServiceResource))
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (dialog is null)
        {
            return new EntityNotFound<DialogEntity>(request.Id);
        }

        _db.TrySetOriginalETag(dialog, request.ETag);

        // Update primitive properties
        _mapper.Map(request.Dto, dialog);
        ValidateTimeFields(dialog);
        await AppendActivity(dialog, request.Dto, cancellationToken);

        dialog.Body = _localizationService.Merge(dialog.Body, request.Dto.Body);
        dialog.Title = _localizationService.Merge(dialog.Title, request.Dto.Title);
        dialog.SenderName = _localizationService.Merge(dialog.SenderName, request.Dto.SenderName);
        
        dialog.SearchTags
            .Merge(request.Dto.SearchTags,
                destinationKeySelector: x => x.Value,
                sourceKeySelector: x => x.Value,
                create: _mapper.Map<List<DialogSearchTag>>,
                delete: DeleteDelegate.NoOp,
                comparer: StringComparer.InvariantCultureIgnoreCase);
        
        await dialog.Elements
            .MergeAsync(request.Dto.Elements,
                destinationKeySelector: x => x.Id,
                sourceKeySelector: x => x.Id,
                create: CreateElements,
                update: UpdateElements,
                delete: DeleteDelegate.NoOp,
                cancellationToken: cancellationToken);

        dialog.GuiActions
            .Merge(request.Dto.GuiActions,
                destinationKeySelector: x => x.Id,
                sourceKeySelector: x => x.Id,
                create: CreateGuiActions,
                update: UpdateGuiActions,
                delete: DeleteDelegate.NoOp);

        dialog.ApiActions
            .Merge(request.Dto.ApiActions,
                destinationKeySelector: x => x.Id,
                sourceKeySelector: x => x.Id,
                create: CreateApiActions,
                update: UpdateApiActions,
                delete: DeleteDelegate.NoOp);


        var saveResult = await _unitOfWork.SaveChangesAsync(cancellationToken);
        return saveResult.Match<UpdateDialogResult>(
            success => success,
            domainError => domainError,
            concurrencyError => concurrencyError);
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
        var newDialogActivities = _mapper.Map<List<DialogActivity>>(dto.Activities);

        var existingIds = await _db.GetExistingIds(newDialogActivities, cancellationToken);
        if (existingIds.Any())
        {
            _domainContext.AddError(
                nameof(UpdateDialogDto.Activities), 
                $"Entity '{nameof(DialogActivity)}' with the following key(s) already exists: ({string.Join(", ", existingIds)}).");
        }

        dialog.Activities.AddRange(newDialogActivities);

        // Tell ef explicitly to add activities as new to the database.
        _db.DialogActivities.AddRange(newDialogActivities);
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

    private IEnumerable<DialogGuiAction> CreateGuiActions(IEnumerable<UpdateDialogDialogGuiActionDto> creatables)
    {
        return creatables.Select(x =>
        {
            var guiAction = _mapper.Map<DialogGuiAction>(x);
            guiAction.Title = _mapper.Map<DialogGuiActionTitle>(x.Title);
            return guiAction;
        });
    }

    private void UpdateGuiActions(IEnumerable<UpdateSet<DialogGuiAction, UpdateDialogDialogGuiActionDto>> updateSets)
    {
        foreach (var (source, destination) in updateSets)
        {
            _mapper.Map(source, destination);
            destination.Title = _localizationService.Merge(destination.Title, source.Title);
        }
    }

    private async Task<IEnumerable<DialogElement>> CreateElements(IEnumerable<UpdateDialogDialogElementDto> creatables, CancellationToken cancellationToken)
    {
        var elements = new List<DialogElement>();
        foreach (var elementDto in creatables)
        {
            var element = _mapper.Map<DialogElement>(elementDto);
            element.DisplayName = _localizationService.Merge(element.DisplayName, elementDto.DisplayName);
            element.Urls = _mapper.Map<List<DialogElementUrl>>(elementDto.Urls);
            elements.Add(element);
        }

        var existingIds = await _db.GetExistingIds(elements, cancellationToken);
        if (existingIds.Any())
        {
            _domainContext.AddError(nameof(UpdateDialogDto.Elements), $"Entity '{nameof(DialogElement)}' with the following key(s) already exists: ({string.Join(", ", existingIds)}).");
        }

        _db.DialogElements.AddRange(elements);
        return elements;
    }

    private Task UpdateElements(IEnumerable<UpdateSet<DialogElement, UpdateDialogDialogElementDto>> updateSets, CancellationToken _)
    {
        foreach (var updateSet in updateSets)
        {
            _mapper.Map(updateSet.Source, updateSet.Destination);
            updateSet.Destination.DisplayName = _localizationService.Merge(updateSet.Destination.DisplayName, updateSet.Source.DisplayName);

            updateSet.Destination.Urls
                .Merge(updateSet.Source.Urls,
                    destinationKeySelector: x => x.Id,
                    sourceKeySelector: x => x.Id,
                    create: _mapper.Map<List<DialogElementUrl>>,
                    update: _mapper.Update,
                    delete: DeleteDelegate.NoOp);
        }

        return Task.CompletedTask;
    }
}
