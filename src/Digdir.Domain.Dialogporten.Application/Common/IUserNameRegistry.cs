using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.Authentication;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;

namespace Digdir.Domain.Dialogporten.Application.Common;

public interface IUserNameRegistry
{
    string GetCurrentUserExternalId();
    Task<UserInformation> GetCurrentUserInformation(CancellationToken cancellationToken);
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
    public string GetCurrentUserExternalId()
    {
        if (_user.TryGetPid(out var userId)) return userId;
        if (_user.TryGetLegacySystemUserId(out userId)) return userId;
        if (_user.TryGetOrgNumber(out userId)) return userId;

        throw new InvalidOperationException("User external id not found");
    }

    public async Task<UserInformation> GetCurrentUserInformation(CancellationToken cancellationToken)
    {
        var userExernalId = GetCurrentUserExternalId();
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
            case UserType.SystemUser:
            // TODO: Implement when we know how system users will be handled
            case UserType.Unknown:
            default:
                // This should never happen as GetCurrentExternalId should throw if the user type is unknown
                throw new UnreachableException();
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
    public string GetCurrentUserExternalId() => _userNameRegistry.GetCurrentUserExternalId();

    public Task<UserInformation> GetCurrentUserInformation(CancellationToken cancellationToken)
        => Task.FromResult(new UserInformation(GetCurrentUserExternalId(), LocalDevelopmentUserPid));
}
