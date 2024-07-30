using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Create;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Update;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Contents;

using GetDialogContentDtoEU = Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Get.GetDialogContentDto;
using GetDialogContentDtoSO = Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Get.GetDialogContentDto;

using SearchDialogContentDtoEU = Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Search.SearchDialogContentDto;
using SearchDialogDtoContentSO = Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Search.SearchDialogContentDto;

using GetDialogDialogTransmissionContentDtoSO = Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Get.GetDialogDialogTransmissionContentDto;
using GetDialogDialogTransmissionContentDtoEU = Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Get.GetDialogDialogTransmissionContentDto;

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
            typeof(CreateDialogContentDto),
            typeof(UpdateDialogContentDto),
            typeof(GetDialogContentDtoEU),
            typeof(GetDialogContentDtoSO)
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
            typeof(SearchDialogContentDtoEU),
            typeof(SearchDialogDtoContentSO),
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
        var transmissionContentTypeNames = TransmissionContentType.GetValues()
            .Select(x => x.Name)
            .ToList();

        var dtoTypes = new[]
        {
            typeof(CreateDialogDialogTransmissionContentDto),
            typeof(UpdateDialogDialogTransmissionContentDto),
            typeof(GetDialogDialogTransmissionContentDtoSO),
            typeof(GetDialogDialogTransmissionContentDtoEU),
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
