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












        const int maxDepth = 100;
        var graph = BuildGraph(dialog.Transmissions);
        var visited = new HashSet<Guid>();
        var stack = new HashSet<Guid>();
        var refCount = new Dictionary<Guid, int>();
        var depthCache = new Dictionary<Guid, int>();

        foreach (var transmission in dialog.Transmissions)
        {
            if (transmission.RelatedTransmissionId.HasValue)
            {
                if (!refCount.TryAdd(transmission.RelatedTransmissionId.Value, 1))
                {
                    refCount[transmission.RelatedTransmissionId.Value]++;
                    _domainContext.AddError(
                        nameof(CreateDialogDto.Transmissions),
                        "TODO: Multiple references to the same transmission");
                    return;
                }
            }

            if (HasCycle(transmission.Id, graph, visited, stack))
            {
                _domainContext.AddError(
                    nameof(CreateDialogDto.Transmissions),
                    $"TODO: Cycle detected in '{nameof(DialogTransmission.RelatedTransmissionId)}' for entity '{nameof(DialogTransmission)}' with key '{transmission.Id}'");
                return;
            }

            if (GetDepth(transmission.Id, graph, depthCache) > maxDepth)
            {
                _domainContext.AddError(
                    nameof(CreateDialogDto.Transmissions),
                    "TODO: Transmission chain depth cannot exceed 100");
            }
        }
    }

    private static int GetDepth(Guid nodeId, Dictionary<Guid, List<Guid>> graph, Dictionary<Guid, int> depthCache)
    {
        if (depthCache.TryGetValue(nodeId, out var cachedDepth))
        {
            return cachedDepth;
        }

        if (graph[nodeId].Count == 0)
        {
            depthCache[nodeId] = 1;
            return 1;
        }

        var depth = 1 + graph[nodeId].Max(neighbor => GetDepth(neighbor, graph, depthCache));
        depthCache[nodeId] = depth;
        return depth;
    }

    private static bool HasCycle(Guid nodeId, Dictionary<Guid, List<Guid>> graph, HashSet<Guid> visited, HashSet<Guid> stack)
    {
        if (stack.Contains(nodeId))
        {
            return true;
        }

        if (!visited.Add(nodeId))
        {
            return false;
        }

        stack.Add(nodeId);

        foreach (var neighbor in graph[nodeId])
        {
            if (HasCycle(neighbor, graph, visited, stack))
            {
                return true;
            }
        }

        stack.Remove(nodeId);
        return false;
    }
    private static Dictionary<Guid, List<Guid>> BuildGraph(List<DialogTransmission> transmissions)
    {
        var graph = new Dictionary<Guid, List<Guid>>();
        foreach (var transmission in transmissions)
        {
            if (!graph.TryGetValue(transmission.Id, out var value))
            {
                value = [];
                graph[transmission.Id] = value;
            }

            if (!transmission.RelatedTransmissionId.HasValue) continue;

            if (!graph.ContainsKey(transmission.RelatedTransmissionId.Value))
            {
                graph[transmission.RelatedTransmissionId.Value] = [];
            }

            value.Add(transmission.RelatedTransmissionId.Value);
        }
        return graph;
    }

}
