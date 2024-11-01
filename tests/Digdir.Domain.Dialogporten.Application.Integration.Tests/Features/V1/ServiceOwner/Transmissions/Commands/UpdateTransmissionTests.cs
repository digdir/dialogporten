using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Update;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.Application.Integration.Tests.Common;
using Digdir.Domain.Dialogporten.Domain.Actors;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions;
using Digdir.Tool.Dialogporten.GenerateFakeData;
using FluentAssertions;
using static Digdir.Domain.Dialogporten.Application.Integration.Tests.UuiDv7Utils;

namespace Digdir.Domain.Dialogporten.Application.Integration.Tests.Features.V1.ServiceOwner.Transmissions.Commands;

[Collection(nameof(DialogCqrsCollectionFixture))]
public class UpdateTransmissionTests : ApplicationCollectionFixture
{
    public UpdateTransmissionTests(DialogApplication application) : base(application) { }

    [Fact]
    public async Task Can_Create_Simple_Transmission_In_Update()
    {
        // Arrange
        var createDialogCommand = DialogGenerator.GenerateSimpleFakeDialog();
        var existingTransmission = DialogGenerator.GenerateFakeDialogTransmissions(1).First();

        createDialogCommand.Transmissions.Add(existingTransmission);
        var createCommandResponse = await Application.Send(createDialogCommand);

        var getDialogQuery = new GetDialogQuery { DialogId = createCommandResponse.AsT0.Value };
        var getDialogDto = await Application.Send(getDialogQuery);

        var mapper = Application.GetMapper();
        var updateDialogDto = mapper.Map<UpdateDialogDto>(getDialogDto.AsT0);

        var newTransmission = UpdateDialogDialogTransmissionDto();
        updateDialogDto.Transmissions.Add(newTransmission);

        // Act
        var updateResponse = await Application.Send(new UpdateDialogCommand
        {
            Id = createCommandResponse.AsT0.Value,
            Dto = updateDialogDto
        });

        // Assert
        updateResponse.TryPickT0(out var success, out _).Should().BeTrue();
        success.Should().NotBeNull();

        var transmissionEntities = await Application.GetDbEntities<DialogTransmission>();
        transmissionEntities.Should().HaveCount(2);
        transmissionEntities.Single(x => x.Id == newTransmission.Id).Should().NotBeNull();
    }

    [Fact]
    public async Task Can_Update_Related_Transmission_With_Null_Id()
    {
        // Arrange
        var createDialogCommand = DialogGenerator.GenerateSimpleFakeDialog();
        var existingTransmission = DialogGenerator.GenerateFakeDialogTransmissions(1).First();
        createDialogCommand.Transmissions.Add(existingTransmission);
        var createCommandResponse = await Application.Send(createDialogCommand);

        var getDialogQuery = new GetDialogQuery { DialogId = createCommandResponse.AsT0.Value };
        var getDialogDto = await Application.Send(getDialogQuery);

        var mapper = Application.GetMapper();
        var updateDialogDto = mapper.Map<UpdateDialogDto>(getDialogDto.AsT0);

        // Add new transmission with null Id
        // This test assures that the Update-handler will use CreateVersion7IfDefault
        // on all transmissions before validating the hierarchy.
        var newTransmission = UpdateDialogDialogTransmissionDto();
        newTransmission.RelatedTransmissionId = existingTransmission.Id;
        newTransmission.Id = null;

        updateDialogDto.Transmissions.Add(newTransmission);

        // Act
        var updateResponse = await Application.Send(new UpdateDialogCommand
        {
            Id = createCommandResponse.AsT0.Value,
            Dto = updateDialogDto
        });

        // Assert
        updateResponse.TryPickT0(out var success, out _).Should().BeTrue();
        success.Should().NotBeNull();
        var transmissionEntities = await Application.GetDbEntities<DialogTransmission>();
        transmissionEntities.Should().HaveCount(2);
        transmissionEntities.Single(x => x.Id == newTransmission.Id).Should().NotBeNull();
    }

    [Fact]
    public async Task Cannot_Include_Old_Transmissions_In_UpdateCommand()
    {
        // Arrange
        var createDialogCommand = DialogGenerator.GenerateSimpleFakeDialog();
        var existingTransmission = DialogGenerator.GenerateFakeDialogTransmissions(count: 1).First();
        createDialogCommand.Transmissions.Add(existingTransmission);
        var createCommandResponse = await Application.Send(createDialogCommand);

        var getDialogQuery = new GetDialogQuery { DialogId = createCommandResponse.AsT0.Value };
        var getDialogDto = await Application.Send(getDialogQuery);

        var mapper = Application.GetMapper();
        var updateDialogDto = mapper.Map<UpdateDialogDto>(getDialogDto.AsT0);

        var newTransmission = UpdateDialogDialogTransmissionDto();
        newTransmission.Id = existingTransmission.Id;
        updateDialogDto.Transmissions.Add(newTransmission);

        // Act
        var updateResponse = await Application.Send(new UpdateDialogCommand
        {
            Id = createCommandResponse.AsT0.Value,
            Dto = updateDialogDto
        });

        // Assert
        updateResponse.TryPickT5(out var domainError, out _).Should().BeTrue();
        domainError.Should().NotBeNull();
        domainError.Errors.Should().Contain(e => e.ErrorMessage.Contains(existingTransmission.Id.ToString()!));
    }

    private static TransmissionDto UpdateDialogDialogTransmissionDto() => new()
    {
        Id = GenerateBigEndianUuidV7(),
        Type = DialogTransmissionType.Values.Information,
        Sender = new() { ActorType = ActorType.Values.ServiceOwner },
        Content = new()
        {
            Title = new() { Value = DialogGenerator.GenerateFakeLocalizations(1) },
            Summary = new() { Value = DialogGenerator.GenerateFakeLocalizations(1) }
        }
    };
}
