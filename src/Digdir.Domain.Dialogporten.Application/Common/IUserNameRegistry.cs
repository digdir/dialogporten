using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;

namespace Digdir.Domain.Dialogporten.Application.Common;

public interface IUserNameRegistry
{
    bool TryGetCurrentUserPid([NotNullWhen(true)] out string? userPid);
    Task<UserInformation?> GetUserInformation(CancellationToken cancellationToken);
}

public record UserInformation(string UserPid, string? UserName);

public class UserNameRegistry : IUserNameRegistry
{
    private readonly IUser _user;
    private readonly INameRegistry _nameRegistry;

    public UserNameRegistry(IUser user, INameRegistry nameRegistry)
    {
        _user = user ?? throw new ArgumentNullException(nameof(user));
        _nameRegistry = nameRegistry ?? throw new ArgumentNullException(nameof(nameRegistry));
    }

    public bool TryGetCurrentUserPid([NotNullWhen(true)] out string? userPid) => _user.TryGetPid(out userPid);

    public async Task<UserInformation?> GetUserInformation(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserPid(out var userPid))
        {
            return null;
        }

        var userName = await _nameRegistry.GetName(userPid, cancellationToken);
        return new(userPid, userName);
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

    public bool TryGetCurrentUserPid([NotNullWhen(true)] out string? userPid) =>
        _userNameRegistry.TryGetCurrentUserPid(out userPid);

    public Task<UserInformation?> GetUserInformation(CancellationToken cancellationToken)
        => _userNameRegistry.TryGetCurrentUserPid(out var userPid)
            ? Task.FromResult<UserInformation?>(new UserInformation(userPid!, LocalDevelopmentUserPid))
            : throw new UnreachableException();
}
