﻿using Castle.Core.Logging;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.Application.Integration.Tests.Common;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions;
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

    [Fact]
    public async Task Can_Create_Dialog_With_UpdatedAt_Supplied()
    {
        // Arrange
        var dialogId = GenerateBigEndianUuidV7();
        var createdAt = DateTimeOffset.UtcNow.AddYears(-20);
        var updatedAt = DateTimeOffset.UtcNow.AddYears(-15);
        var createDialogCommand = DialogGenerator.GenerateFakeDialog(id: dialogId, updatedAt: updatedAt, createdAt: createdAt);

        // Act
        var createDialogResult = await Application.Send(createDialogCommand);
        var getDialogQuery = new GetDialogQuery
        {
            DialogId = dialogId
        };

        var getDialogResponse = await Application.Send(getDialogQuery);

        // Assert
        createDialogResult.TryPickT0(out var dialogCreatedSuccess, out _).Should().BeTrue();
        dialogCreatedSuccess.Value.Should().Be(dialogId);

        getDialogQuery.Should().NotBeNull();
        getDialogResponse.TryPickT0(out var dialog, out _).Should().BeTrue();
        dialog.Should().NotBeNull();
        dialog.CreatedAt.Should().BeCloseTo(createdAt, precision: TimeSpan.FromMicroseconds(10));
        dialog.UpdatedAt.Should().BeCloseTo(updatedAt, precision: TimeSpan.FromMicroseconds(10));
    }

    [Fact]
    public async Task Cant_Create_Dialog_With_UpdatedAt_Supplied_Without_CreatedAt_Supplied()
    {
        // Arrange
        var updatedAt = DateTimeOffset.UtcNow.AddYears(-15);
        var createDialogCommand = DialogGenerator.GenerateFakeDialog(updatedAt: updatedAt);

        // Act
        var response = await Application.Send(createDialogCommand);

        // Assert
        response.TryPickT2(out var validationError, out _).Should().BeTrue();
        validationError.Should().NotBeNull();
        validationError.Errors.Should().Contain(e => e.ErrorMessage.Contains(nameof(createDialogCommand.UpdatedAt)));
    }

    [Fact]
    public async Task Cant_Create_Dialog_With_UpdatedAt_Date_Earlier_Than_CreatedAt_Date()
    {
        // Arrange
        var createdAt = DateTimeOffset.UtcNow.AddYears(-10);
        var updatedAt = DateTimeOffset.UtcNow.AddYears(-15);
        var createDialogCommand = DialogGenerator.GenerateFakeDialog(updatedAt: updatedAt, createdAt: createdAt);

        // Act
        var response = await Application.Send(createDialogCommand);

        // Assert
        response.TryPickT2(out var validationError, out _).Should().BeTrue();
        validationError.Should().NotBeNull();
        validationError.Errors.Should().Contain(e => e.ErrorMessage.Contains(nameof(createDialogCommand.CreatedAt)));
    }

    [Fact]
    public async Task Cant_Create_Dialog_With_UpdatedAt_Or_CreatedAt_In_The_Future()
    {
        // Arrange
        var aYearFromNow = DateTimeOffset.UtcNow.AddYears(1);
        var createDialogCommand = DialogGenerator
            .GenerateFakeDialog(updatedAt: aYearFromNow, createdAt: aYearFromNow);

        // Act
        var response = await Application.Send(createDialogCommand);

        //
        response.TryPickT2(out var validationError, out _).Should().BeTrue();
        validationError.Should().NotBeNull();
        validationError.Errors.Should().Contain(e => e.ErrorMessage.Contains("in the past"));
    }

    [Fact]
    public async Task Can_Create_Dialog_With_UpdatedAt_And_CreatedAt_Being_Equal()
    {
        // Arrange
        var aYearAgo = DateTimeOffset.UtcNow.AddYears(-1);
        var createDialogCommand = DialogGenerator
            .GenerateFakeDialog(updatedAt: aYearAgo, createdAt: aYearAgo);

        // Act
        var response = await Application.Send(createDialogCommand);

        // Assert
        response.TryPickT0(out var success, out _).Should().BeTrue();
        success.Should().NotBeNull();
    }

    [Fact]
    public async Task Cannot_Create_Transmission_Without_Content()
    {
        // Arrange
        var transmission = DialogGenerator.GenerateFakeDialogTransmissions(1)[0];
        transmission.Content = null!;

        var createDialogCommand = DialogGenerator.GenerateSimpleFakeDialog();
        createDialogCommand.Transmissions = [transmission];

        // Act
        var response = await Application.Send(createDialogCommand);

        // Assert
        response.TryPickT2(out var validationError, out _).Should().BeTrue();
        validationError.Should().NotBeNull();
        validationError.Errors.Should().HaveCount(1);
        validationError.Errors.First().ErrorMessage.Should().Contain("'Content' must not be empty");
    }

    [Fact]
    public async Task Cannot_Create_Transmission_Without_Content_Value()
    {
        // Arrange
        var transmission = DialogGenerator.GenerateFakeDialogTransmissions(1)[0];
        transmission.Content.Summary.Value = [];
        transmission.Content.Title.Value = [];

        var createDialogCommand = DialogGenerator.GenerateSimpleFakeDialog();
        createDialogCommand.Transmissions = [transmission];

        // Act
        var response = await Application.Send(createDialogCommand);

        // Assert
        response.TryPickT2(out var validationError, out _).Should().BeTrue();
        validationError.Should().NotBeNull();
        validationError.Errors
            .Count(e => e.PropertyName.Contains(nameof(createDialogCommand.Content)))
            .Should()
            .Be(2);
    }

    [Fact]
    public async Task Cannot_Create_Transmission_With_Empty_Content_Localization_Values()
    {
        // Arrange
        var transmission = DialogGenerator.GenerateFakeDialogTransmissions(1)[0];
        transmission.Content.Summary.Value = [new LocalizationDto { LanguageCode = "nb", Value = "" }];
        transmission.Content.Title.Value = [new LocalizationDto { LanguageCode = "nb", Value = "" }];

        var createDialogCommand = DialogGenerator.GenerateSimpleFakeDialog();
        createDialogCommand.Transmissions = [transmission];

        // Act
        var response = await Application.Send(createDialogCommand);

        // Assert
        response.TryPickT2(out var validationError, out _).Should().BeTrue();
        validationError.Should().NotBeNull();
        validationError.Errors
            .Count(e => e.PropertyName.Contains(nameof(createDialogCommand.Content)))
            .Should()
            .Be(2);
    }
}
