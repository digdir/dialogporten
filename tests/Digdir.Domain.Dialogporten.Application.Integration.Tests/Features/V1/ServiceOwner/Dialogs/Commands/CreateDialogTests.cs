using Digdir.Domain.Dialogporten.Application.Integration.Tests.Common;
using Digdir.Tool.Dialogporten.GenerateFakeData;
using FluentAssertions;
using static Digdir.Domain.Dialogporten.Application.Integration.Tests.UuiDv7Utils;

namespace Digdir.Domain.Dialogporten.Application.Integration.Tests.Features.V1.ServiceOwner.Dialogs.Commands;

[Collection(nameof(DialogCqrsCollectionFixture))]
public class CreateDialogTests : ApplicationCollectionFixture
{
    public CreateDialogTests(DialogApplication application) : base(application) { }

    [Fact]
    public async Task Cant_Create_Dialog_With_UUIDv4_format()
    {
        // Arrange
        var invalidDialogId = Guid.NewGuid();

        var createDialogCommand = DialogGenerator.GenerateSimpleFakeDialog(id: invalidDialogId);

        // Act
        var response = await Application.Send(createDialogCommand);

        // Assert
        response.TryPickT2(out var validationError, out _).Should().BeTrue();
        validationError.Should().NotBeNull();
    }

    [Fact]
    public async Task Cant_Create_Dialog_With_UUIDv7_In_Little_Endian_Format()
    {
        // Arrange
        // Guid created with Medo, Uuid7.NewUuid7().ToGuid()
        var invalidDialogId = Guid.Parse("638e9101-6bc7-7975-b392-ba5c5a528c23");

        var createDialogCommand = DialogGenerator.GenerateSimpleFakeDialog(id: invalidDialogId);

        // Act
        var response = await Application.Send(createDialogCommand);

        // Assert
        response.TryPickT2(out var validationError, out _).Should().BeTrue();
        validationError.Should().NotBeNull();
    }

    [Fact]
    public async Task Cant_Create_Dialog_With_ID_With_Timestamp_In_The_Future()
    {
        // Arrange
        var timestamp = DateTimeOffset.UtcNow.AddSeconds(1);
        var invalidDialogId = GenerateBigEndianUuidV7(timestamp);

        var createDialogCommand = DialogGenerator.GenerateSimpleFakeDialog(id: invalidDialogId);

        // Act
        var response = await Application.Send(createDialogCommand);

        // Assert
        response.TryPickT2(out var validationError, out _).Should().BeTrue();
        validationError.Should().NotBeNull();
    }

    [Fact]
    public async Task Create_Dialog_With_ID_With_Timestamp_In_The_Past()
    {
        // Arrange
        var timestamp = DateTimeOffset.UtcNow.AddSeconds(-1);
        var validDialogId = GenerateBigEndianUuidV7(timestamp);

        var createDialogCommand = DialogGenerator.GenerateSimpleFakeDialog(id: validDialogId);

        // Act
        var response = await Application.Send(createDialogCommand);

        // Assert
        response.TryPickT0(out var success, out _).Should().BeTrue();
        success.Should().NotBeNull();
        success.Value.Should().Be(validDialogId);
    }

    [Fact]
    public async Task Create_CreatesDialog_WhenDialogIsSimple()
    {
        // Arrange
        var expectedDialogId = GenerateBigEndianUuidV7();
        var createCommand = DialogGenerator.GenerateSimpleFakeDialog(id: expectedDialogId);

        // Act
        var response = await Application.Send(createCommand);

        // Assert
        response.TryPickT0(out var success, out _).Should().BeTrue();
        success.Value.Should().Be(expectedDialogId);
    }

    [Fact]
    public async Task Create_CreateDialog_WhenDialogIsComplex()
    {
        // Arrange
        var expectedDialogId = GenerateBigEndianUuidV7();
        var createDialogCommand = DialogGenerator.GenerateFakeDialog(id: expectedDialogId);

        // Act
        var result = await Application.Send(createDialogCommand);

        // Assert
        result.TryPickT0(out var success, out _).Should().BeTrue();
        success.Value.Should().Be(expectedDialogId);
    }

    // TODO: Add tests
}
