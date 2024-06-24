using System.Diagnostics;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;
using UserIdType = Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogUserType.Values;

namespace Digdir.Domain.Dialogporten.Application.Common;

public interface IUserRegistry
{
    UserId GetCurrentUserId();
    Task<UserInformation> GetCurrentUserInformation(CancellationToken cancellationToken);
}

public sealed class UserId
{
    public required UserIdType Type { get; set; }
    public required string ExternalId { get; init; }
}

public sealed class UserInformation
{
    public required UserId UserId { get; init; }
    public string? Name { get; init; }
}

public class UserRegistry : IUserRegistry
{
    private readonly IUser _user;
    private readonly IPersonNameRegistry _personNameRegistry;

    public UserRegistry(
        IUser user,
        IPersonNameRegistry personNameRegistry)
    {
        _user = user ?? throw new ArgumentNullException(nameof(user));
        _personNameRegistry = personNameRegistry ?? throw new ArgumentNullException(nameof(personNameRegistry));
    }

    public UserId GetCurrentUserId()
    {
        var (userType, externalId) = _user.GetPrincipal().GetUserType();
        if (userType == UserIdType.Unknown)
        {
            throw new InvalidOperationException("User external id not found");
        }

        return new() { Type = userType, ExternalId = externalId };
    }

    public async Task<UserInformation> GetCurrentUserInformation(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var name = userId.Type switch
        {
            UserIdType.Person or UserIdType.ServiceOwnerOnBehalfOfPerson => await _personNameRegistry.GetName(userId.ExternalId, cancellationToken),
            UserIdType.SystemUser => "System User",// TODO: Implement when SystemUsers are introduced?
            UserIdType.Unknown => throw new UnreachableException(),
            UserIdType.ServiceOwner => throw new UnreachableException(),
            _ => throw new UnreachableException(),
        };
        return new()
        {
            UserId = userId,
            Name = name,
        };
    }
}

internal sealed class LocalDevelopmentUserRegistryDecorator : IUserRegistry
{
    private const string LocalDevelopmentUserName = "Local Development User";
    private readonly IUserRegistry _userRegistry;

    public LocalDevelopmentUserRegistryDecorator(IUserRegistry userRegistry)
    {
        _userRegistry = userRegistry ?? throw new ArgumentNullException(nameof(userRegistry));
    }

    public UserId GetCurrentUserId() => _userRegistry.GetCurrentUserId();

    public Task<UserInformation> GetCurrentUserInformation(CancellationToken cancellationToken)
        => Task.FromResult(new UserInformation
        {
            UserId = GetCurrentUserId(),
            Name = LocalDevelopmentUserName
        });
}
