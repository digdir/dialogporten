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
    public async Task Cannot_Update_Transmission_Hierarchy_With_Breadth_Depth_And_Circular_Reference_Errors()
    {
        // Arrange
        var createDialogCommand = DialogGenerator.GenerateSimpleFakeDialog();
        var createCommandResponse = await Application.Send(createDialogCommand);

        var getDialogQuery = new GetDialogQuery { DialogId = createCommandResponse.AsT0.Value };
        var getDialogDto = await Application.Send(getDialogQuery);

        var mapper = Application.GetMapper();
        var updateDialogDto = mapper.Map<UpdateDialogDto>(getDialogDto.AsT0);

        var transmissions = Enumerable.Range(0, 110)
            .Select(_ => UpdateDialogDialogTransmissionDto())
            .ToList();

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

        updateDialogDto.Transmissions = transmissions;

        // Act
        var updateResponse = await Application.Send(new UpdateDialogCommand
        {
            Id = createCommandResponse.AsT0.Value,
            Dto = updateDialogDto
        });

        // Assert
        updateResponse.TryPickT5(out var domainError, out _).Should().BeTrue();
        domainError.Should().NotBeNull();
        domainError.Errors.Should().NotBeEmpty();
        domainError.Errors.Should().HaveCount(3);
    }

    [Fact]
    public async Task Cannot_Update_Transmission_Hierarchy_With_Multiple_Circular_References()
    {
        // Arrange
        var createDialogCommand = DialogGenerator.GenerateSimpleFakeDialog();
        var createCommandResponse = await Application.Send(createDialogCommand);

        var getDialogQuery = new GetDialogQuery { DialogId = createCommandResponse.AsT0.Value };
        var getDialogDto = await Application.Send(getDialogQuery);

        var mapper = Application.GetMapper();
        var updateDialogDto = mapper.Map<UpdateDialogDto>(getDialogDto.AsT0);

        var transmissions = Enumerable.Range(0, 6)
            .Select(_ => UpdateDialogDialogTransmissionDto())
            .ToList();

        transmissions[0].RelatedTransmissionId = transmissions[1].Id;
        transmissions[1].RelatedTransmissionId = transmissions[2].Id;
        transmissions[2].RelatedTransmissionId = transmissions[0].Id; // First cycle

        transmissions[3].RelatedTransmissionId = transmissions[4].Id;
        transmissions[4].RelatedTransmissionId = transmissions[5].Id;
        transmissions[5].RelatedTransmissionId = transmissions[3].Id; // Second cycle

        updateDialogDto.Transmissions = transmissions;

        // Act
        var updateResponse = await Application.Send(new UpdateDialogCommand
        {
            Id = createCommandResponse.AsT0.Value,
            Dto = updateDialogDto
        });

        // Assert
        updateResponse.TryPickT5(out var domainError, out _).Should().BeTrue();
        domainError.Errors.Should().NotBeEmpty();
        // domainError.Errors.Should().HaveCount(2); ???
        domainError.Errors.First().ErrorMessage.Should().Contain(transmissions[0].Id!.Value.ToString());
    }

    [Fact]
    public async Task Cannot_Update_Transmission_Hierarchy_With_Multiple_Width_Violations()
    {
        // Arrange
        var createDialogCommand = DialogGenerator.GenerateSimpleFakeDialog();
        var createCommandResponse = await Application.Send(createDialogCommand);

        var getDialogQuery = new GetDialogQuery { DialogId = createCommandResponse.AsT0.Value };
        var getDialogDto = await Application.Send(getDialogQuery);

        var mapper = Application.GetMapper();
        var updateDialogDto = mapper.Map<UpdateDialogDto>(getDialogDto.AsT0);

        const int widthViolationSize = 5;
        const int totalTransmissions = 2 * widthViolationSize;

        var transmissions = Enumerable.Range(0, totalTransmissions)
            .Select(_ => UpdateDialogDialogTransmissionDto())
            .ToList();

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

        updateDialogDto.Transmissions = transmissions;

        // Act
        var updateResponse = await Application.Send(new UpdateDialogCommand
        {
            Id = createCommandResponse.AsT0.Value,
            Dto = updateDialogDto
        });

        // Assert
        updateResponse.TryPickT5(out var domainError, out _).Should().BeTrue();
        domainError.Should().NotBeNull();
        domainError.Errors.Should().NotBeEmpty();
        domainError.Errors.First().ErrorMessage.Should().Contain(transmissions[0].Id!.Value.ToString());
        domainError.Errors.First().ErrorMessage.Should()
            .Contain(transmissions[widthViolationSize].Id!.Value.ToString());
    }

    [Fact]
    public async Task Cannot_Update_Transmission_Hierarchy_With_Multiple_Depth_Violations()
    {
        // Arrange
        var createDialogCommand = DialogGenerator.GenerateSimpleFakeDialog();
        var createCommandResponse = await Application.Send(createDialogCommand);

        var getDialogQuery = new GetDialogQuery { DialogId = createCommandResponse.AsT0.Value };
        var getDialogDto = await Application.Send(getDialogQuery);

        var mapper = Application.GetMapper();
        var updateDialogDto = mapper.Map<UpdateDialogDto>(getDialogDto.AsT0);

        const int depthViolationLength = 102;
        const int totalTransmissions = 2 * depthViolationLength;

        var initialTransmission = UpdateDialogDialogTransmissionDto();
        updateDialogDto.Transmissions.Add(initialTransmission);

        // First depth violation
        for (var i = 1; i < depthViolationLength; i++)
        {
            var newTransmission = UpdateDialogDialogTransmissionDto();
            newTransmission.RelatedTransmissionId = updateDialogDto.Transmissions[i - 1].Id;
            updateDialogDto.Transmissions.Add(newTransmission);
        }

        // Second depth violation
        for (var i = depthViolationLength; i < totalTransmissions; i++)
        {
            var newTransmission = UpdateDialogDialogTransmissionDto();
            newTransmission.RelatedTransmissionId = updateDialogDto.Transmissions[i - 1].Id;
            updateDialogDto.Transmissions.Add(newTransmission);
        }

        // Act
        var updateResponse = await Application.Send(new UpdateDialogCommand
        {
            Id = createCommandResponse.AsT0.Value,
            Dto = updateDialogDto
        });

        // Assert
        updateResponse.TryPickT5(out var domainError, out _).Should().BeTrue();
        domainError.Should().NotBeNull();
        domainError.Errors.Should().NotBeEmpty();
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

    [Fact]
    public async Task Cannot_Update_Transmission_Referencing_Non_Existent_Id()
    {
        // Arrange
        var createDialogCommand = DialogGenerator.GenerateSimpleFakeDialog();
        var createCommandResponse = await Application.Send(createDialogCommand);

        var getDialogQuery = new GetDialogQuery { DialogId = createCommandResponse.AsT0.Value };
        var getDialogDto = await Application.Send(getDialogQuery);

        var mapper = Application.GetMapper();
        var updateDialogDto = mapper.Map<UpdateDialogDto>(getDialogDto.AsT0);

        var newTransmission = UpdateDialogDialogTransmissionDto();
        newTransmission.RelatedTransmissionId = GenerateBigEndianUuidV7(); // Non-existent ID

        updateDialogDto.Transmissions = [newTransmission];

        // Act
        var updateResponse = await Application.Send(new UpdateDialogCommand
        {
            Id = createCommandResponse.AsT0.Value,
            Dto = updateDialogDto
        });

        // Assert
        updateResponse.TryPickT2(out var validationError, out _).Should().BeTrue();
        validationError.Should().NotBeNull();
        // validationError.Errors.First().ErrorMessage.Should().Contain("exist");
    }


    [Fact]
    public async Task Cannot_Update_Transmission_Hierarchy_With_Self_Reference()
    {
        // Arrange
        var createDialogCommand = DialogGenerator.GenerateSimpleFakeDialog();
        var createCommandResponse = await Application.Send(createDialogCommand);

        var getDialogQuery = new GetDialogQuery { DialogId = createCommandResponse.AsT0.Value };
        var getDialogDto = await Application.Send(getDialogQuery);

        var mapper = Application.GetMapper();
        var updateDialogDto = mapper.Map<UpdateDialogDto>(getDialogDto.AsT0);

        // Create a transmission with self-reference
        var newTransmission = UpdateDialogDialogTransmissionDto();
        newTransmission.RelatedTransmissionId = newTransmission.Id; // Self-reference
        updateDialogDto.Transmissions.Add(newTransmission);

        // Act
        var updateResponse = await Application.Send(new UpdateDialogCommand
        {
            Id = createCommandResponse.AsT0.Value,
            Dto = updateDialogDto
        });

        // Assert
        updateResponse.TryPickT3(out var validationError, out _).Should().BeTrue();
        validationError.Should().NotBeNull();
        validationError.Errors.Should().HaveCount(1);
        validationError.Errors.First().ErrorMessage.Should().Contain(newTransmission.Id!.Value.ToString());
    }

    [Fact]
    public async Task Can_Handle_Large_Transmission_Hierarchy_With_Varying_Chain_Lengths_Update()
    {
        // Arrange
        var createDialogCommand = DialogGenerator.GenerateSimpleFakeDialog();
        var createCommandResponse = await Application.Send(createDialogCommand);

        var getDialogQuery = new GetDialogQuery { DialogId = createCommandResponse.AsT0.Value };
        var getDialogDto = await Application.Send(getDialogQuery);

        var mapper = Application.GetMapper();
        var updateDialogDto = mapper.Map<UpdateDialogDto>(getDialogDto.AsT0);

        const int transmissionCount = 500;
        var transmissions = Enumerable.Range(0, transmissionCount)
            .Select(_ => UpdateDialogDialogTransmissionDto())
            .ToList();

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

        updateDialogDto.Transmissions = transmissions;

        // Act
        var updateResponse = await Application.Send(new UpdateDialogCommand
        {
            Id = createCommandResponse.AsT0.Value,
            Dto = updateDialogDto
        });

        // Assert
        updateResponse.TryPickT0(out var success, out _).Should().BeTrue();

        var transmissionEntities = await Application.GetDbEntities<DialogTransmission>();
        transmissionEntities.Should().HaveCount(transmissionCount);
        transmissionEntities.Should().OnlyContain(x => x.DialogId.Equals(createCommandResponse.AsT0.Value));
    }

    private static UpdateDialogDialogTransmissionDto UpdateDialogDialogTransmissionDto() => new()
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
