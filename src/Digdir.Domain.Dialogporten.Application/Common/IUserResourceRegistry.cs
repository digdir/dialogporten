using System.Diagnostics;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;

namespace Digdir.Domain.Dialogporten.Application.Common;

public interface IUserResourceRegistry
{
    Task<bool> CurrentUserIsOwner(string serviceResource, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<string>> GetCurrentUserResourceIds(CancellationToken cancellationToken);
}

public class UserResourceRegistry : IUserResourceRegistry
{
    private readonly IUser _user;
    private readonly IResourceRegistry _resourceRegistry;

    public UserResourceRegistry(IUser user, IResourceRegistry resourceRegistry)
    {
        _user = user ?? throw new ArgumentNullException(nameof(user));
        _resourceRegistry = resourceRegistry ?? throw new ArgumentNullException(nameof(resourceRegistry));
    }

    public async Task<bool> CurrentUserIsOwner(string serviceResource, CancellationToken cancellationToken)
    {
        var resourceIds = await GetCurrentUserResourceIds(cancellationToken);
        return resourceIds.Contains(serviceResource);
    }

    public Task<IReadOnlyCollection<string>> GetCurrentUserResourceIds(CancellationToken cancellationToken) =>
        !_user.TryGetOrgNumber(out var orgNumber)
            ? throw new UnreachableException()
            : _resourceRegistry.GetResourceIds(orgNumber, cancellationToken);
}

internal sealed class LocalDevelopmentUserResourceRegistryDecorator : IUserResourceRegistry
{
    private readonly IUserResourceRegistry _userResourceRegistry;

    public LocalDevelopmentUserResourceRegistryDecorator(IUserResourceRegistry userResourceRegistry)
    {
        _userResourceRegistry = userResourceRegistry ?? throw new ArgumentNullException(nameof(userResourceRegistry));
    }

    public Task<bool> CurrentUserIsOwner(string serviceResource, CancellationToken cancellationToken) =>
        Task.FromResult(true);

    public Task<IReadOnlyCollection<string>> GetCurrentUserResourceIds(CancellationToken cancellationToken) =>
        _userResourceRegistry.GetCurrentUserResourceIds(cancellationToken);
}
