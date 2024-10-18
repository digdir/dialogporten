using Digdir.Domain.Dialogporten.Domain.Common;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.Transmissions;

public static class TransmissionHierarchyValidator
{
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

            if (!transmissionChildrenIdByParentId.TryGetValue(potentialChild.RelatedTransmissionId.Value,
                    out var parentContext))
            {
                transmissionChildrenIdByParentId[potentialChild.RelatedTransmissionId.Value] =
                    parentContext = new() { Id = potentialChild.Id };
            }

            parentContext.Children.Add(potentialChild.RelatedTransmissionId.Value);
        }

        return transmissionChildrenIdByParentId;
    }

    private sealed class HierarchyContext
    {
        public required Guid Id { get; init; }
        public List<Guid> Children { get; } = [];
        public int Depth { get; set; }
        public bool PartOfCycle { get; set; }
    }
}
