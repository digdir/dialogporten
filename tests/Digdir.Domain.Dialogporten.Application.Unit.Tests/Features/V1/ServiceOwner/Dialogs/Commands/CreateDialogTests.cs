using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.ResourceRegistry;
using Digdir.Domain.Dialogporten.Application.Common.Services;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Create;
using Digdir.Tool.Dialogporten.GenerateFakeData;
using NSubstitute;
using AuthorizationConstants = Digdir.Domain.Dialogporten.Application.Common.Authorization.Constants;

namespace Digdir.Domain.Dialogporten.Application.Unit.Tests.Features.V1.ServiceOwner.Dialogs.Commands;

public class CreateDialogTests
{
    [Fact]
    public async Task CreateDialogCommand_Should_Return_Forbidden_When_Scope_Is_Missing()
    {
        // Arrange
        var dialogDbContextSub = Substitute.For<Externals.IDialogDbContext>();

        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(CreateDialogCommandHandler).Assembly);
        }).CreateMapper();

        var unitOfWorkSub = Substitute.For<Externals.IUnitOfWork>();
        var domainContextSub = Substitute.For<IDomainContext>();
        var userResourceRegistrySub = Substitute.For<IUserResourceRegistry>();
        var userOrganizationRegistrySub = Substitute.For<IUserOrganizationRegistry>();
        var activityServiceSub = Substitute.For<IDialogActivityService>();

        var createCommand = DialogGenerator.GenerateSimpleFakeDialog();

        userResourceRegistrySub
            .CurrentUserIsOwner(createCommand.ServiceResource, Arg.Any<CancellationToken>())
            .Returns(true);

        userResourceRegistrySub.GetResourceType(createCommand.ServiceResource, Arg.Any<CancellationToken>())
            .Returns(Constants.Correspondence);

        var commandHandler = new CreateDialogCommandHandler(dialogDbContextSub,
            mapper, unitOfWorkSub, domainContextSub, userResourceRegistrySub,
            userOrganizationRegistrySub, activityServiceSub);

        // Act
        var result = await commandHandler.Handle(createCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsT3);
        Assert.Contains(AuthorizationConstants.CorrespondenceScope, result.AsT3.Reasons[0]);
    }


    [Fact]
    public async Task CreateDialogCommand_Should_Return_ValidationError_When_Progress_Set_On_Correspondence()
    {
        // Arrange
        var dialogDbContextSub = Substitute.For<Externals.IDialogDbContext>();

        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(CreateDialogCommandHandler).Assembly);
        }).CreateMapper();

        var unitOfWorkSub = Substitute.For<Externals.IUnitOfWork>();
        var domainContextSub = Substitute.For<IDomainContext>();
        var userResourceRegistrySub = Substitute.For<IUserResourceRegistry>();
        var userOrganizationRegistrySub = Substitute.For<IUserOrganizationRegistry>();
        var activityServiceSub = Substitute.For<IDialogActivityService>();

        var createCommand = DialogGenerator.GenerateSimpleFakeDialog();

        userResourceRegistrySub
            .CurrentUserIsOwner(createCommand.ServiceResource, Arg.Any<CancellationToken>())
            .Returns(true);

        userResourceRegistrySub.UserCanModifyResourceType(Arg.Any<string>()).Returns(true);

        userResourceRegistrySub.GetResourceType(createCommand.ServiceResource, Arg.Any<CancellationToken>())
            .Returns(Constants.Correspondence);

        var commandHandler = new CreateDialogCommandHandler(dialogDbContextSub,
            mapper, unitOfWorkSub, domainContextSub, userResourceRegistrySub,
            userOrganizationRegistrySub, activityServiceSub);

        // Act
        var result = await commandHandler.Handle(createCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsT2); // ValidationError
        Assert.Equal(CreateDialogCommandHandler.ProgressValidationFailure.ErrorMessage,
            result.AsT2.Errors.First().ErrorMessage);
    }
}
