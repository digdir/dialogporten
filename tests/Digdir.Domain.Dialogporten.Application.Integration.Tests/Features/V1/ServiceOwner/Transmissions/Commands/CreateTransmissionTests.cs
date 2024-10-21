using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Content;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Application.Integration.Tests.Common;
using Digdir.Domain.Dialogporten.Domain;
using Digdir.Domain.Dialogporten.Domain.Common;
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
    public async Task Can_Create_Related_Transmission_With_Null_Id()
    {
        // Arrange
        var createCommand = DialogGenerator.GenerateSimpleFakeDialog();
        var transmissions = DialogGenerator.GenerateFakeDialogTransmissions(2);

        transmissions[0].RelatedTransmissionId = transmissions[1].Id;

        // This test assures that the Create-handler will use CreateVersion7IfDefault
        // on all transmissions before validating the hierarchy.
        transmissions[0].Id = null;

        createCommand.Transmissions = transmissions;

        // Act
        var response = await Application.Send(createCommand);

        // Assert
        response.TryPickT0(out var success, out _).Should().BeTrue();
        success.Should().NotBeNull();
    }

    [Fact]
    public async Task Cannot_Create_Transmission_Hierarchy_With_Breadth_Depth_And_Circular_Reference_Errors()
    {
        // Arrange
        var createCommand = DialogGenerator.GenerateSimpleFakeDialog();

        var transmissions = DialogGenerator.GenerateFakeDialogTransmissions(110);

        // Breadth constraint
        transmissions[1].RelatedTransmissionId = transmissions[0].Id;
        transmissions[2].RelatedTransmissionId = transmissions[0].Id;

        // Depth constraint
        for (var i = 3; i < transmissions.Count; i++)
        {
            transmissions[i].RelatedTransmissionId = transmissions[i - 1].Id;
        }

        // Circular reference
        transmissions[107].RelatedTransmissionId = transmissions[108].Id;
        transmissions[108].RelatedTransmissionId = transmissions[109].Id;
        transmissions[109].RelatedTransmissionId = transmissions[107].Id;

        createCommand.Transmissions = transmissions;

        // Act
        var response = await Application.Send(createCommand);

        // Assert
        response.TryPickT1(out var domainError, out _).Should().BeTrue();
        domainError.Errors.Should().HaveCount(3);
    }

    [Fact]
    public async Task Cannot_Create_Transmission_Hierarchy_With_Multiple_Circular_References()
    {
        // Arrange
        var createCommand = DialogGenerator.GenerateSimpleFakeDialog();

        var transmissions = DialogGenerator.GenerateFakeDialogTransmissions(6);
        transmissions[0].RelatedTransmissionId = transmissions[1].Id;
        transmissions[1].RelatedTransmissionId = transmissions[2].Id;
        transmissions[2].RelatedTransmissionId = transmissions[0].Id; // First cycle

        transmissions[3].RelatedTransmissionId = transmissions[4].Id;
        transmissions[4].RelatedTransmissionId = transmissions[5].Id;
        transmissions[5].RelatedTransmissionId = transmissions[3].Id; // Second cycle

        createCommand.Transmissions = transmissions;

        // Act
        var response = await Application.Send(createCommand);

        // Assert
        response.TryPickT1(out var domainError, out _).Should().BeTrue();
        domainError.Errors.Should().NotBeEmpty();
        // domainError.Errors.Should().HaveCount(2); // Expecting two cycle errors?
    }

    [Fact]
    public async Task Cannot_Create_Transmission_Hierarchy_With_Multiple_Width_Violations()
    {
        // Arrange
        var createCommand = DialogGenerator.GenerateSimpleFakeDialog();
        const int widthViolationSize = 5;
        const int totalTransmissions = 2 * widthViolationSize;

        var transmissions = DialogGenerator.GenerateFakeDialogTransmissions(totalTransmissions);

        // First width violation
        for (var i = 1; i < widthViolationSize; i++)
        {
            transmissions[i].RelatedTransmissionId = transmissions[0].Id;
        }

        // Second width violation
        for (var i = widthViolationSize + 1; i < totalTransmissions; i++)
        {
            transmissions[i].RelatedTransmissionId = transmissions[widthViolationSize].Id;
        }

        createCommand.Transmissions = transmissions;

        // Act
        var response = await Application.Send(createCommand);

        // Assert
        response.TryPickT1(out var domainError, out _).Should().BeTrue();
        domainError.Errors.Should().NotBeEmpty();
        // domainError.Errors.Should().HaveCount(2); // Expecting two width errors?
    }

    [Fact]
    public async Task Cannot_Create_Transmission_Hierarchy_With_Multiple_Depth_Violations()
    {
        // Arrange
        var createCommand = DialogGenerator.GenerateSimpleFakeDialog();
        const int depthViolationLength = 102;
        const int totalTransmissions = 2 * depthViolationLength;

        var transmissions = DialogGenerator.GenerateFakeDialogTransmissions(totalTransmissions);

        // First depth violation
        for (var i = 1; i < depthViolationLength; i++)
        {
            transmissions[i].RelatedTransmissionId = transmissions[i - 1].Id;
        }

        // Second depth violation
        for (var i = depthViolationLength + 1; i < totalTransmissions; i++)
        {
            transmissions[i].RelatedTransmissionId = transmissions[i - 1].Id;
        }

        createCommand.Transmissions = transmissions;

        // Act
        var response = await Application.Send(createCommand);

        // Assert
        response.TryPickT1(out var domainError, out _).Should().BeTrue();
        domainError.Errors.Should().NotBeEmpty();
        // domainError.Errors.Should().HaveCount(2); // Expecting two depth errors?
    }

    [Fact]
    public async Task Cannot_Create_Transmission_Referencing_Non_Existent_Id()
    {
        // Arrange
        var createCommand = DialogGenerator.GenerateSimpleFakeDialog();

        var transmission = DialogGenerator.GenerateFakeDialogTransmissions(1)[0];
        transmission.RelatedTransmissionId = GenerateBigEndianUuidV7(); // Non-existent ID

        createCommand.Transmissions = [transmission];

        // Act
        var response = await Application.Send(createCommand);

        // Assert
        response.TryPickT2(out var validationError, out _).Should().BeTrue();
        validationError.Errors.Should().HaveCount(1);
        validationError.Errors.First().ErrorMessage.Should().Contain(transmission.RelatedTransmissionId.Value.ToString());
    }

    [Fact]
    public async Task Cannot_Create_Transmission_Hierarchy_With_Self_Reference()
    {
        // Arrange
        var createCommand = DialogGenerator.GenerateSimpleFakeDialog();

        var transmission = DialogGenerator.GenerateFakeDialogTransmissions(1)[0];
        transmission.RelatedTransmissionId = transmission.Id;

        createCommand.Transmissions = [transmission];

        // Act
        var response = await Application.Send(createCommand);

        // Assert
        response.TryPickT2(out var validationError, out _).Should().BeTrue();
        validationError.Errors.Should().HaveCount(1);
        validationError.Errors.First().ErrorMessage.Should().Contain(transmission.Id!.Value.ToString());
    }

    [Fact]
    public async Task Can_Handle_Large_Transmission_Hierarchy_With_Varying_Chain_Lengths()
    {
        // Arrange
        var createCommand = DialogGenerator.GenerateSimpleFakeDialog();
        const int transmissionCount = 500;
        var transmissions = DialogGenerator.GenerateFakeDialogTransmissions(transmissionCount);
        var chainStartIndex = 0;
        int[] chainLengths = [10, 20, 30, 40, 50, 60, 70, 80, 90, 100];

        while (chainStartIndex < transmissions.Count)
        {
            // Determine the length of the next chain from the predefined pattern
            var remainingTransmissions = transmissions.Count - chainStartIndex;
            var chainLength = chainLengths[chainStartIndex % chainLengths.Length];
            chainLength = Math.Min(chainLength, remainingTransmissions);

            // Create the chain
            var chainEndIndex = chainStartIndex + chainLength - 1;
            for (var i = chainStartIndex + 1; i <= chainEndIndex; i++)
            {
                transmissions[i].RelatedTransmissionId = transmissions[i - 1].Id;
            }

            // Move to the next chain
            chainStartIndex = chainEndIndex + 1;
        }

        createCommand.Transmissions = transmissions;

        // Act
        var response = await Application.Send(createCommand);

        // Assert
        response.TryPickT0(out var success, out _).Should().BeTrue();
        UuidV7.IsValid(success.Value).Should().BeTrue();

        var transmissionEntities = await Application.GetDbEntities<DialogTransmission>();
        transmissionEntities.Should().HaveCount(transmissionCount);
        transmissionEntities.Should().OnlyContain(x => x.DialogId.Equals(success.Value));
    }
}
