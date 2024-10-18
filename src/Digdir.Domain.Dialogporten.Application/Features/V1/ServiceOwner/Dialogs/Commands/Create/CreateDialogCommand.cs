using System.Diagnostics;
using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.Authorization;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;
using Digdir.Domain.Dialogporten.Domain.Actors;
using Digdir.Domain.Dialogporten.Domain.Common;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions;
using Digdir.Domain.Dialogporten.Domain.Parties;
using Digdir.Library.Entity.Abstractions.Features.Identifiable;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Create;

public sealed class CreateDialogCommand : CreateDialogDto, IRequest<CreateDialogResult>;

[GenerateOneOf]
public sealed partial class CreateDialogResult : OneOfBase<Success<Guid>, DomainError, ValidationError, Forbidden>;

internal sealed class CreateDialogCommandHandler : IRequestHandler<CreateDialogCommand, CreateDialogResult>
{
    private readonly IDialogDbContext _db;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainContext _domainContext;
    private readonly IUserOrganizationRegistry _userOrganizationRegistry;
    private readonly IServiceResourceAuthorizer _serviceResourceAuthorizer;
    private readonly IUser _user;

    public CreateDialogCommandHandler(
        IUser user,
        IDialogDbContext db,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        IDomainContext domainContext,
        IUserOrganizationRegistry userOrganizationRegistry,
        IServiceResourceAuthorizer serviceResourceAuthorizer)
    {
        _user = user ?? throw new ArgumentNullException(nameof(user));
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _domainContext = domainContext ?? throw new ArgumentNullException(nameof(domainContext));
        _userOrganizationRegistry = userOrganizationRegistry ?? throw new ArgumentNullException(nameof(userOrganizationRegistry));
        _serviceResourceAuthorizer = serviceResourceAuthorizer;
    }

    public async Task<CreateDialogResult> Handle(CreateDialogCommand request, CancellationToken cancellationToken)
    {
        var dialog = _mapper.Map<DialogEntity>(request);

        await _serviceResourceAuthorizer.SetResourceType(dialog, cancellationToken);
        var serviceResourceAuthorizationResult = await _serviceResourceAuthorizer.AuthorizeServiceResources(dialog, cancellationToken);
        if (serviceResourceAuthorizationResult.Value is Forbidden forbiddenResult)
        {
            return forbiddenResult;
        }

        dialog.Org = await _userOrganizationRegistry.GetCurrentUserOrgShortName(cancellationToken) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(dialog.Org))
        {
            _domainContext.AddError(new DomainFailure(nameof(DialogEntity.Org),
                "Cannot find service owner organization shortname for current user. Please ensure that you are logged in as a service owner."));
        }
        CreateDialogEndUserContext(request, dialog);
        await EnsureNoExistingUserDefinedIds(dialog, cancellationToken);
        await _db.Dialogs.AddAsync(dialog, cancellationToken);
        var saveResult = await _unitOfWork.SaveChangesAsync(cancellationToken);
        return saveResult.Match<CreateDialogResult>(
            success => new Success<Guid>(dialog.Id),
            domainError => domainError,
            concurrencyError => throw new UnreachableException("Should never get a concurrency error when creating a new dialog"));
    }

    private void CreateDialogEndUserContext(CreateDialogCommand request, DialogEntity dialog)
    {
        dialog.DialogEndUserContext = new();
        if (!request.SystemLabel.HasValue)
        {
            return;
        }

        if (!_user.TryGetOrganizationNumber(out var organizationNumber))
        {
            _domainContext.AddError(new DomainFailure(nameof(organizationNumber), "Cannot find organization number for current user."));
            return;
        }

        dialog.DialogEndUserContext.UpdateLabel(
            request.SystemLabel.Value,
            $"{NorwegianOrganizationIdentifier.PrefixWithSeparator}{organizationNumber}",
            ActorType.Values.ServiceOwner);
    }

    private async Task EnsureNoExistingUserDefinedIds(DialogEntity dialog, CancellationToken cancellationToken)
    {
        var existingDialogIds = await _db.GetExistingIds([dialog], cancellationToken);
        if (existingDialogIds.Count != 0)
        {
            _domainContext.AddError(DomainFailure.EntityExists<DialogEntity>(existingDialogIds));
        }

        var existingActivityIds = await _db.GetExistingIds(dialog.Activities, cancellationToken);
        if (existingActivityIds.Count != 0)
        {
            _domainContext.AddError(DomainFailure.EntityExists<DialogActivity>(existingActivityIds));
        }

        var existingTransmissionIds = await _db.GetExistingIds(dialog.Transmissions, cancellationToken);
        if (existingTransmissionIds.Count != 0)
        {
            _domainContext.AddError(DomainFailure.EntityExists<DialogTransmission>(existingTransmissionIds));
        }

        var existingTransmissionAttachmentIds = await _db.GetExistingIds(dialog.Transmissions.SelectMany(t => t.Attachments), cancellationToken);
        if (existingTransmissionAttachmentIds.Count != 0)
        {
            _domainContext.AddError(DomainFailure.EntityExists<DialogTransmissionAttachment>(existingTransmissionAttachmentIds));
        }

        _domainContext.AddErrors(ValidateTransmissionHierarchy(dialog.Transmissions));


        //
        //
        //
        //
        //
        //
        //
        // var moreThanOneYo = dialog.Transmissions
        //     .Where(x => x.RelatedTransmissionId is not null)
        //     .GroupBy(x => x.RelatedTransmissionId)
        //     .Where(x => x.Count() > 1)
        //     .Select(x => x.Key)
        //     .ToList();
        //
        // if (moreThanOneYo.Count != 0)
        // {
        //     // domain err
        // }
        //
        //
        //
        //
        //
        //
        //
        //
        //
        //
        //
        //
        //
        // // set transmission id
        //
        // const int maxDepth = 100;
        // var transmissionChildrenIdByParentId = BuildGraph(dialog.Transmissions);
        //
        // // Breadth
        // var refCount = new HashSet<Guid>();
        //
        // var visited = new HashSet<Guid>();
        // var stack = new HashSet<Guid>();
        // var depthCache = new Dictionary<Guid, int>();
        //
        // foreach (var transmission in dialog.Transmissions)
        // {
        //     if (transmission.RelatedTransmissionId.HasValue)
        //     {
        //         if (!refCount.Add(transmission.RelatedTransmissionId.Value))
        //         {
        //             _domainContext.AddError(
        //                 nameof(CreateDialogDto.Transmissions),
        //                 "TODO: Multiple references to the same transmission");
        //         }
        //     }
        //
        //     if (HasCycle(transmission.Id, transmissionChildrenIdByParentId, visited, stack))
        //     {
        //         _domainContext.AddError(
        //             nameof(CreateDialogDto.Transmissions),
        //             $"TODO: Cycle detected in '{nameof(DialogTransmission.RelatedTransmissionId)}' for entity '{nameof(DialogTransmission)}' with key '{transmission.Id}'");
        //         return;
        //     }
        //
        //     if (GetDepth(transmission.Id, transmissionChildrenIdByParentId, depthCache) > maxDepth)
        //     {
        //         _domainContext.AddError(
        //             nameof(CreateDialogDto.Transmissions),
        //             "TODO: Transmission chain depth cannot exceed 100");
        //         return;
        //     }
        // }
    }

    public static List<DomainFailure> ValidateTransmissionHierarchy(List<DialogTransmission> transmissions)
    {
        var breadCrumbs = new HashSet<Guid>();
        var visited = new HashSet<Guid>();
        var hierarchyContextById = ToHierarchyContextById(transmissions);

        foreach (var (parentId, parentContext) in hierarchyContextById)
        {
            if (visited.Contains(parentId))
            {
                continue;
            }

            Analyze(parentContext, hierarchyContextById, visited, breadCrumbs);
        }

        var widthErrors = hierarchyContextById
            .Where(x => x.Value.Children.Count > 1)
            .Select(x => new DomainFailure(nameof(DialogEntity.Transmissions),
                $"Hierarchy width exceeded for '{nameof(DialogTransmission)}' with key '{x.Key}'. Expected one reference but found {x.Value.Children.Count}."))
            .ToList();

        var depthErrors = hierarchyContextById
            .Where(x => x.Value.Depth == 101)
            .Select(x => new DomainFailure(nameof(DialogEntity.Transmissions),
                $"Hierarchy depth exceeded for '{nameof(DialogTransmission)}' with key '{x.Key}'. Expected depth of 100 but found {x.Value.Depth}."))
            .ToList();

        var cycleErrors = hierarchyContextById
            .Where(x => x.Value.PartOfCycle)
            .Select(x => new DomainFailure(nameof(DialogEntity.Transmissions),
                $"Cycle detected in '{nameof(DialogTransmission.RelatedTransmissionId)}' for entity '{nameof(DialogTransmission)}' with key '{x.Key}'"))
            .ToList();

        return widthErrors
            .Concat(depthErrors)
            .Concat(cycleErrors)
            .ToList();
    }

    private static void Analyze(
        HierarchyContext context,
        Dictionary<Guid, HierarchyContext> hierarchyContextById,
        HashSet<Guid> visited,
        HashSet<Guid> breadCrumbs)
    {
        if (breadCrumbs.Contains(context.Id))
        {
            context.PartOfCycle = true;
            return;
        }

        if (!visited.Add(context.Id))
        {
            return;
        }

        breadCrumbs.Add(context.Id);

        foreach (var childId in context.Children)
        {
            var childContext = hierarchyContextById[childId];
            Analyze(childContext, hierarchyContextById, visited, breadCrumbs);
            context.PartOfCycle |= childContext.PartOfCycle;
            context.Depth = Math.Max(context.Depth, 1 + childContext.Depth);
        }

        breadCrumbs.Remove(context.Id);
    }

    // private static int GetDepth(Guid parentId, Dictionary<Guid, List<Guid>> childByParentId, Dictionary<Guid, int> depthCache)
    // {
    //     if (depthCache.TryGetValue(parentId, out var cachedDepth))
    //     {
    //         return cachedDepth;
    //     }
    //
    //     if (childByParentId[parentId].Count == 0)
    //     {
    //         return depthCache[parentId] = 1;
    //     }
    //
    //     var children = childByParentId[parentId];
    //     var depth = 1 + children.Max(child => GetDepth(child, childByParentId, depthCache));
    //     depthCache[parentId] = depth;
    //     return depth;
    // }
    //
    // private static bool HasCycle(Guid parentId, Dictionary<Guid, List<Guid>> childByParentId, HashSet<Guid> visited, HashSet<Guid> breadCrumbs)
    // {
    //     if (breadCrumbs.Contains(parentId))
    //     {
    //         return true;
    //     }
    //
    //     if (!visited.Add(parentId))
    //     {
    //         return false;
    //     }
    //
    //     breadCrumbs.Add(parentId);
    //
    //     var children = childByParentId[parentId];
    //     if (children.Any(child => HasCycle(child, childByParentId, visited, breadCrumbs)))
    //     {
    //         return true;
    //     }
    //
    //     breadCrumbs.Remove(parentId);
    //     return false;
    // }

    private static Dictionary<Guid, HierarchyContext> ToHierarchyContextById(List<DialogTransmission> transmissions)
    {
        var transmissionChildrenIdByParentId = new Dictionary<Guid, HierarchyContext>();
        foreach (var potentialChild in transmissions)
        {
            if (!transmissionChildrenIdByParentId.ContainsKey(potentialChild.Id))
            {
                transmissionChildrenIdByParentId[potentialChild.Id] = new() { Id = potentialChild.Id };
            }

            if (!potentialChild.RelatedTransmissionId.HasValue)
            {
                continue;
            }

            if (!transmissionChildrenIdByParentId.TryGetValue(potentialChild.RelatedTransmissionId.Value, out var parentContext))
            {
                transmissionChildrenIdByParentId[potentialChild.RelatedTransmissionId.Value] = parentContext = new() { Id = potentialChild.RelatedTransmissionId.Value };
            }

            parentContext.Children.Add(potentialChild.RelatedTransmissionId.Value);
        }

        return transmissionChildrenIdByParentId;
    }

    public class HierarchyContext
    {
        public required Guid Id { get; init; }
        public List<Guid> Children { get; } = [];
        public int Depth { get; set; }
        public bool PartOfCycle { get; set; }
    }
}
