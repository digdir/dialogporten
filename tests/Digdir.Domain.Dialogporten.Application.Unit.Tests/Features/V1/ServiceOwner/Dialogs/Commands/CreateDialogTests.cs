using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.Authorization;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Create;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Tool.Dialogporten.GenerateFakeData;
using NSubstitute;

namespace Digdir.Domain.Dialogporten.Application.Unit.Tests.Features.V1.ServiceOwner.Dialogs.Commands;

public class CreateDialogTests
{
    [Fact]
    public async Task CreateDialogCommand_Should_Return_Forbidden_When_Scope_Is_Missing()
    {
        // Arrange
        var dialogDbContextSub = Substitute.For<IDialogDbContext>();

        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(CreateDialogCommandHandler).Assembly);
        }).CreateMapper();

        var unitOfWorkSub = Substitute.For<IUnitOfWork>();
        var domainContextSub = Substitute.For<IDomainContext>();
        var userResourceRegistrySub = Substitute.For<IUserResourceRegistry>();
        var userOrganizationRegistrySub = Substitute.For<IUserOrganizationRegistry>();
        var serviceAuthorizationSub = Substitute.For<IServiceResourceAuthorizer>();

        var createCommand = DialogGenerator.GenerateSimpleFakeDialog();

        serviceAuthorizationSub
            .AuthorizeServiceResources(Arg.Any<DialogEntity>(), Arg.Any<CancellationToken>())
            .Returns(new Forbidden());

        userResourceRegistrySub
            .CurrentUserIsOwner(createCommand.ServiceResource, Arg.Any<CancellationToken>())
            .Returns(true);

        var commandHandler = new CreateDialogCommandHandler(dialogDbContextSub,
            mapper, unitOfWorkSub, domainContextSub,
            userOrganizationRegistrySub, serviceAuthorizationSub);

        // Act
        var result = await commandHandler.Handle(createCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsT3);
    }

    [Fact]
    public async Task CreateDialogCommand_Should_Return_Forbidden_When_User_Is_Not_Owner()
    {
        // Arrange
        var dialogDbContextSub = Substitute.For<IDialogDbContext>();

        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(CreateDialogCommandHandler).Assembly);
        }).CreateMapper();

        var unitOfWorkSub = Substitute.For<IUnitOfWork>();
        var domainContextSub = Substitute.For<IDomainContext>();
        var userResourceRegistrySub = Substitute.For<IUserResourceRegistry>();
        var userOrganizationRegistrySub = Substitute.For<IUserOrganizationRegistry>();
        var serviceAuthorizationSub = Substitute.For<IServiceResourceAuthorizer>();

        var createCommand = DialogGenerator.GenerateSimpleFakeDialog();

        serviceAuthorizationSub
            .AuthorizeServiceResources(Arg.Any<DialogEntity>(), Arg.Any<CancellationToken>())
            .Returns(new Forbidden());

        userResourceRegistrySub
            .CurrentUserIsOwner(createCommand.ServiceResource, Arg.Any<CancellationToken>())
            .Returns(false);

        var commandHandler = new CreateDialogCommandHandler(dialogDbContextSub,
            mapper, unitOfWorkSub, domainContextSub,
            userOrganizationRegistrySub, serviceAuthorizationSub);

        // Act
        var result = await commandHandler.Handle(createCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsT3); // Forbidden
    }
}
