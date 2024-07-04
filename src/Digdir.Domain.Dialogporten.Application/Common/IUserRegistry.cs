using System.Diagnostics;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Parties;

namespace Digdir.Domain.Dialogporten.Application.Common;

public interface IUserRegistry
{
    UserId GetCurrentUserId();
    Task<UserInformation> GetCurrentUserInformation(CancellationToken cancellationToken);
}

public sealed class UserId
{
    public required ActorType.Values Type { get; set; }
    public required string ExternalId { get; init; }
    public required string URNId { get; init; }
}

public sealed class UserInformation
{
    public required UserId UserId { get; init; }
    public string? Name { get; init; }
    public string? URNId { get; init; }
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
        if (userType == ActorType.Values.Unknown)
        {
            throw new InvalidOperationException("User external id not found");
        }

        return new() { Type = userType, ExternalId = externalId, URNId = $"{NorwegianPersonIdentifier.PrefixWithSeparator}{externalId}" };
    }

    public async Task<UserInformation> GetCurrentUserInformation(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        string? name;
        string? urnId;

        switch (userId.Type)
        {
            case ActorType.Values.Person:
            case ActorType.Values.UserViaServiceOwner:
                name = await _personNameRegistry.GetName(userId.ExternalId, cancellationToken);
                // todo: would it be correct to set the URNId here? Or does it belong elsewhere...
                urnId = $"{NorwegianPersonIdentifier.PrefixWithSeparator}{userId.ExternalId}";
                break;

            case ActorType.Values.LegacySystemUser:
                _user.TryGetLegacySystemUserName(out var legacyUserName);
                name = legacyUserName;
                urnId = $"{NorwegianPersonIdentifier.PrefixWithSeparator}{userId.ExternalId}";
                break;

            case ActorType.Values.SystemUser:
                // TODO: Implement when SystemUsers are introduced?
                // TODO: What would be the URN of the system user?
                name = "System User";
                urnId = $"{NorwegianPersonIdentifier.PrefixWithSeparator}{userId.ExternalId}";
                break;

            case ActorType.Values.ServiceOwner:
            case ActorType.Values.Unknown:
            default:
                throw new UnreachableException();
        }

        return new()
        {
            UserId = userId,
            Name = name,
            URNId = urnId
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
