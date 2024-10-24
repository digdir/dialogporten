using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Unit.Tests.Common;
using Digdir.Domain.Dialogporten.Domain.Common;
using FluentAssertions;

namespace Digdir.Domain.Dialogporten.Application.Unit.Tests;

public class ValidateReferenceHierarchyTests
{
    [Fact]
    public void ThisIsATest()
    {
        // Arrange
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();
        var guid3 = Guid.NewGuid();
        var guid4 = Guid.NewGuid();
        var guid5 = Guid.NewGuid();
        var guid6 = Guid.NewGuid();
        var elements = HierarchyTestNodeBuilder
            .CreateNewHierarchy(10, guid1, guid2).AddWidth(5, guid3)
            .FromNode(guid3).AddDepth(5).AddWidth(3)
            .CreateNewHierarchy(101, guid4, guid5, guid6)
            .FromNode(guid6).AddWidth(3)
            .Build();

        // Act
        var result = Sut(elements, maxDepth: 102, maxWidth: 100);

        // Assert
        result.Should().HaveCount(0);
    }

    private static List<DomainFailure> Sut(IReadOnlyCollection<HierarchyTestNode> nodes, int maxDepth, int maxWidth)
        => nodes.ValidateReferenceHierarchy(
            keySelector: x => x.Id,
            parentKeySelector: x => x.ParentId,
            propertyName: "Reference",
            maxDepth: maxDepth,
            maxWidth: maxWidth);
}
