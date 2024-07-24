using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Search;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Content;
using Digdir.Domain.Dialogporten.GraphQL.EndUser.DialogById;

namespace Digdir.Domain.Dialogporten.GraphQl.Unit.Tests.ObjectTypes;

public class ContentTypeTests
{
    [Fact]
    public void DialogContentType_Names_Should_Match_Props_On_Content()
    {
        // Arrange
        var dialogContentTypeNames = DialogContentType.GetValues()
            .Select(x => x.Name)
            .ToList();

        var dtoPropertyNames = typeof(Content)
            .GetProperties()
            .Select(p => p.Name)
            .ToList();

        Assert.Equal(dialogContentTypeNames.Count, dtoPropertyNames.Count);
        foreach (var contentTypeName in dialogContentTypeNames)
        {
            Assert.Contains(contentTypeName, dtoPropertyNames, StringComparer.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void OutPutInList_DialogContentType_Names_Should_Match_Props_On_SearchContent()
    {
        // Arrange
        var dialogContentTypeNames = DialogContentType.GetValues()
            .Where(x => x.OutputInList)
            .Select(x => x.Name)
            .ToList();

        var dtoPropertyNames = typeof(SearchDialogContentDto)
            .GetProperties()
            .Select(p => p.Name)
            .ToList();

        // Assert
        Assert.Equal(dialogContentTypeNames.Count, dtoPropertyNames.Count);
        foreach (var contentTypeName in dialogContentTypeNames)
        {
            Assert.Contains(contentTypeName, dtoPropertyNames, StringComparer.OrdinalIgnoreCase);
        }
    }
}
