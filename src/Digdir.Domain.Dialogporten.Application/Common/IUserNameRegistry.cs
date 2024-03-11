using System.Diagnostics.CodeAnalysis;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;

namespace Digdir.Domain.Dialogporten.Application.Common;

public interface IUserNameRegistry
{
    bool TryGetCurrentUserPid([NotNullWhen(true)] out string? userPid);
    Task<string?> GetCurrentUserName(string personalIdentificationNumber, CancellationToken cancellationToken);
}

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

    public async Task<string?> GetCurrentUserName(string personalIdentificationNumber, CancellationToken cancellationToken) =>
        await _nameRegistry.GetName(personalIdentificationNumber, cancellationToken);
}

internal sealed class LocalDevelopmentUserNameRegistryDecorator : IUserNameRegistry
{
    private readonly IUserNameRegistry _userNameRegistry;

    public LocalDevelopmentUserNameRegistryDecorator(IUserNameRegistry userNameRegistry)
    {
        _userNameRegistry = userNameRegistry ?? throw new ArgumentNullException(nameof(userNameRegistry));
    }

    public bool TryGetCurrentUserPid([NotNullWhen(true)] out string? userPid) =>
        _userNameRegistry.TryGetCurrentUserPid(out userPid);

    public async Task<string?> GetCurrentUserName(string personalIdentificationNumber, CancellationToken cancellationToken)
        => await Task.FromResult("Local Development User");
}
