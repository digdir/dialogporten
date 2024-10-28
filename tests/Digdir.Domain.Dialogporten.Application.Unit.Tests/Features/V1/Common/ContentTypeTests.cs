using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Create;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Update;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Contents;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions.Contents;
using ContentDto = Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Create.ContentDto;
using DialogTransmissionContentDto = Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Create.DialogTransmissionContentDto;

namespace Digdir.Domain.Dialogporten.Application.Unit.Tests.Features.V1.Common;

public class ContentTypeTests
{
    [Fact]
    public void DialogContentType_Names_Should_Match_Props_On_All_DTOs()
    {
        // Arrange
        var dialogContentTypeNames = DialogContentType.GetValues()
            .Select(x => x.Name)
            .ToList();

        var dtoTypes = new[]
        {
            typeof(ContentDto),
            typeof(Application.Features.V1.ServiceOwner.Dialogs.Commands.Update.ContentDto),
            typeof(Application.Features.V1.EndUser.Dialogs.Queries.Get.ContentDto),
            typeof(Application.Features.V1.ServiceOwner.Dialogs.Queries.Get.ContentDto)
        };

        foreach (var dtoType in dtoTypes)
        {
            var dtoPropertyNames = dtoType.GetProperties()
                .Select(p => p.Name)
                .ToList();

            Assert.Equal(dialogContentTypeNames.Count, dtoPropertyNames.Count);
            foreach (var contentTypeName in dialogContentTypeNames)
            {
                Assert.Contains(contentTypeName, dtoPropertyNames, StringComparer.OrdinalIgnoreCase);
            }
        }
    }

    [Fact]
    public void OutPutInList_DialogContentType_Names_Should_Match_Props_On_All_Search_DTOs()
    {
        // Arrange
        var dialogContentTypeNames = DialogContentType.GetValues()
            .Where(x => x.OutputInList)
            .Select(x => x.Name)
            .ToList();

        var dtoTypes = new[]
        {
            typeof(Application.Features.V1.EndUser.Dialogs.Queries.Search.ContentDto),
            typeof(Application.Features.V1.ServiceOwner.Dialogs.Queries.Search.ContentDto)
        };

        foreach (var dtoType in dtoTypes)
        {
            var dtoPropertyNames = dtoType.GetProperties()
                .Select(p => p.Name)
                .ToList();

            Assert.Equal(dialogContentTypeNames.Count, dtoPropertyNames.Count);
            foreach (var contentTypeName in dialogContentTypeNames)
            {
                Assert.Contains(contentTypeName, dtoPropertyNames, StringComparer.OrdinalIgnoreCase);
            }
        }
    }

    [Fact]
    public void TransmissionContentType_Names_Should_Match_Props_On_All_DTOs()
    {
        // Arrange
        var transmissionContentTypeNames = DialogTransmissionContentType.GetValues()
            .Select(x => x.Name)
            .ToList();

        var dtoTypes = new[]
        {
            typeof(DialogTransmissionContentDto),
            typeof(Application.Features.V1.ServiceOwner.Dialogs.Commands.Update.DialogTransmissionContentDto),
            typeof(Application.Features.V1.ServiceOwner.Dialogs.Queries.Get.DialogTransmissionContentDto),
            typeof(Application.Features.V1.EndUser.Dialogs.Queries.Get.DialogTransmissionContentDto)
        };

        foreach (var dtoType in dtoTypes)
        {
            var dtoPropertyNames = dtoType.GetProperties()
                .Select(p => p.Name)
                .ToList();

            Assert.Equal(transmissionContentTypeNames.Count, dtoPropertyNames.Count);
            foreach (var contentTypeName in transmissionContentTypeNames)
            {
                Assert.Contains(contentTypeName, dtoPropertyNames, StringComparer.OrdinalIgnoreCase);
            }
        }
    }
}
