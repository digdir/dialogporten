using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Content;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Create;
using Digdir.Domain.Dialogporten.Application.Integration.Tests.Common;
using Digdir.Domain.Dialogporten.Domain;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions.Contents;
using Digdir.Tool.Dialogporten.GenerateFakeData;
using FluentAssertions;
using static Digdir.Domain.Dialogporten.Application.Integration.Tests.UuiDv7Utils;

namespace Digdir.Domain.Dialogporten.Application.Integration.Tests.Features.V1.ServiceOwner.Transmissions.Commands;

[Collection(nameof(DialogCqrsCollectionFixture))]
public class CreateTransmissionTests : ApplicationCollectionFixture
{
    public CreateTransmissionTests(DialogApplication application) : base(application) { }

    [Fact]
    public async Task Can_Create_Simple_Transmission()
    {
        // Arrange
        var dialogId = GenerateBigEndianUuidV7();
        var createCommand = DialogGenerator.GenerateSimpleFakeDialog(id: dialogId);

        var transmission = DialogGenerator.GenerateFakeDialogTransmissions(1)[0];
        createCommand.Transmissions = [transmission];

        // Act
        var response = await Application.Send(createCommand);

        // Assert
        response.TryPickT0(out var success, out _).Should().BeTrue();
        success.Value.Should().Be(dialogId);
        var transmissionEntities = await Application.GetDbEntities<DialogTransmission>();
        transmissionEntities.Should().HaveCount(1);
        transmissionEntities.First().DialogId.Should().Be(dialogId);
        transmissionEntities.First().Id.Should().Be(transmission.Id!.Value);
    }

    [Fact]
    public async Task Can_Create_Transmission_With_Embeddable_Content()
    {
        // Arrange
        var dialogId = GenerateBigEndianUuidV7();
        var createCommand = DialogGenerator.GenerateSimpleFakeDialog(id: dialogId);

        var transmissionId = GenerateBigEndianUuidV7();
        var transmission = DialogGenerator.GenerateFakeDialogTransmissions(1)[0];

        const string contentUrl = "https://example.com/transmission";
        transmission.Id = transmissionId;
        transmission.Content.ContentReference = new ContentValueDto
        {
            MediaType = MediaTypes.EmbeddableMarkdown,
            Value = [new LocalizationDto { LanguageCode = "nb", Value = contentUrl }]
        };

        createCommand.Transmissions = [transmission];

        // Act
        var response = await Application.Send(createCommand);

        // Assert
        response.TryPickT0(out var success, out _).Should().BeTrue();
        success.Value.Should().Be(dialogId);

        var transmissionEntities = await Application.GetDbEntities<DialogTransmission>();
        transmissionEntities.Should().HaveCount(1);

        var transmissionEntity = transmissionEntities.First();
        transmissionEntity.DialogId.Should().Be(dialogId);
        transmissionEntity.Id.Should().Be(transmissionId);

        var contentEntities = await Application.GetDbEntities<DialogTransmissionContent>();
        contentEntities.First(x => x.MediaType == MediaTypes.EmbeddableMarkdown).TransmissionId.Should().Be(transmissionId);
    }

    [Fact]
    public async Task Cannot_Create_Transmission_Embeddable_Content_With_Http_Url()
    {
        // Arrange
        var createCommand = DialogGenerator.GenerateSimpleFakeDialog();

        var transmission = DialogGenerator.GenerateFakeDialogTransmissions(1)[0];

        transmission.Content.ContentReference = new ContentValueDto
        {
            MediaType = MediaTypes.EmbeddableMarkdown,
            Value = [new LocalizationDto { LanguageCode = "nb", Value = "http://example.com/transmission" }]
        };

        createCommand.Transmissions = [transmission];

        // Act
        var response = await Application.Send(createCommand);

        // Assert
        response.TryPickT2(out var validationError, out _).Should().BeTrue();
        validationError.Errors.Should().HaveCount(1);
        validationError.Errors.First().ErrorMessage.Should().Contain("HTTPS");

    }

    [Fact]
    public async Task Cannot_Create_Transmission_Reference_Tree_With_Breadth()
    {
        // Arrange
        var createCommand = DialogGenerator.GenerateSimpleFakeDialog();

        var transmissions = DialogGenerator.GenerateFakeDialogTransmissions(3);
        transmissions[0].RelatedTransmissionId = transmissions[2].Id;
        transmissions[1].RelatedTransmissionId = transmissions[2].Id;

        createCommand.Transmissions = transmissions;

        // Act
        var response = await Application.Send(createCommand);

        // Assert
        response.TryPickT2(out var validationError, out _).Should().BeTrue();
        validationError.Errors.Should().Contain(e => e.ErrorMessage.Contains("only one transmission can point to the same relatedTransmissionId"));
    }

    [Fact]
    public async Task Cannot_Create_Transmission_Reference_Tree_With_Circular_Reference()
    {
        // Arrange
        var dialogId = GenerateBigEndianUuidV7();
        var createCommand = DialogGenerator.GenerateSimpleFakeDialog(id: dialogId);

        var transmissions = DialogGenerator.GenerateFakeDialogTransmissions(3);
        transmissions[0].RelatedTransmissionId = transmissions[1].Id;
        transmissions[1].RelatedTransmissionId = transmissions[2].Id;
        transmissions[2].RelatedTransmissionId = transmissions[0].Id;

        createCommand.Transmissions = transmissions;

        // Act
        var response = await Application.Send(createCommand);

        // Assert
        response.TryPickT2(out var validationError, out _).Should().BeTrue();
        validationError.Errors.Should().Contain(e => e.ErrorMessage.Contains("circular references are not allowed"));
    }

    [Fact]
    public async Task Cannot_Create_Transmission_Exceeding_Max_Depth()
    {
        // Arrange
        var createCommand = DialogGenerator.GenerateSimpleFakeDialog();

        var transmissions = DialogGenerator.GenerateFakeDialogTransmissions(101);
        for (var i = 1; i < transmissions.Count; i++)
        {
            transmissions[i].RelatedTransmissionId = transmissions[i - 1].Id;
        }

        createCommand.Transmissions = transmissions;

        // Act
        var response = await Application.Send(createCommand);

        // Assert
        response.TryPickT2(out var validationError, out _).Should().BeTrue();
        validationError.Errors.Should().Contain(e => e.ErrorMessage.Contains("transmission chain depth cannot exceed 100"));
    }
}
