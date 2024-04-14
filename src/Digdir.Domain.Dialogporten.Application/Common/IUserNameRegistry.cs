using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.Authentication;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;

namespace Digdir.Domain.Dialogporten.Application.Common;

public interface IUserNameRegistry
{
    bool TryGetCurrentUserExternalId([NotNullWhen(true)] out string? userExternalId);
    Task<UserInformation?> GetUserInformation(CancellationToken cancellationToken);
}

public record UserInformation(string UserPid, string? UserName);

public class UserNameRegistry : IUserNameRegistry
{
    private readonly IUser _user;
    private readonly INameRegistry _nameRegistry;
    private readonly IOrganizationRegistry _organizationRegistry;

    public UserNameRegistry(IUser user, INameRegistry nameRegistry, IOrganizationRegistry organizationRegistry)
    {
        _user = user ?? throw new ArgumentNullException(nameof(user));
        _nameRegistry = nameRegistry ?? throw new ArgumentNullException(nameof(nameRegistry));
        _organizationRegistry = organizationRegistry ?? throw new ArgumentNullException(nameof(organizationRegistry));
    }

    public bool TryGetCurrentUserExternalId([NotNullWhen(true)] out string? userExternalId)
    {
        if (_user.TryGetPid(out userExternalId)) return true;
        if (_user.TryGetLegacySystemUserId(out userExternalId)) return true;
        if (_user.TryGetOrgNumber(out userExternalId)) return true;
        return false;
    }

    public async Task<UserInformation?> GetUserInformation(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserExternalId(out var userExernalId))
        {
            return null;
        }

        string? userName;
        switch (_user.GetPrincipal().GetUserType())
        {
            case UserType.Person:
                userName = await _nameRegistry.GetName(userExernalId, cancellationToken);
                break;
            case UserType.LegacySystemUser:
                _user.TryGetLegacySystemUserName(out userName);
                break;
            case UserType.Enterprise:
                userName = await _organizationRegistry.GetOrgShortName(userExernalId, cancellationToken);
                break;
            case UserType.Unknown:
            case UserType.SystemUser: // Implement when we know how this will be handled
            default:
                throw new UnreachableException("Unknown user type");
        }

        return new(userExernalId, userName);
    }
}

internal sealed class LocalDevelopmentUserNameRegistryDecorator : IUserNameRegistry
{
    private const string LocalDevelopmentUserPid = "Local Development User";

    private readonly IUserNameRegistry _userNameRegistry;

    public LocalDevelopmentUserNameRegistryDecorator(IUserNameRegistry userNameRegistry)
    {
        _userNameRegistry = userNameRegistry ?? throw new ArgumentNullException(nameof(userNameRegistry));
    }

    public bool TryGetCurrentUserExternalId([NotNullWhen(true)] out string? userExternalId) =>
        _userNameRegistry.TryGetCurrentUserExternalId(out userExternalId);

    public Task<UserInformation?> GetUserInformation(CancellationToken cancellationToken)
        => _userNameRegistry.TryGetCurrentUserExternalId(out var userPid)
            ? Task.FromResult<UserInformation?>(new UserInformation(userPid!, LocalDevelopmentUserPid))
            : throw new UnreachableException();
}
