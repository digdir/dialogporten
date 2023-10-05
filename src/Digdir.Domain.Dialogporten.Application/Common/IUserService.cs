using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;
using Digdir.Domain.Dialogporten.Application.Externals;
using System.Diagnostics;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;

namespace Digdir.Domain.Dialogporten.Application.Common;

internal interface IUserService
{
    Task<bool> CurrentUserIsOwner(string serviceResource, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<string>> GetCurrentUserResourceIds(CancellationToken cancellationToken);
}

internal sealed class UserService : IUserService
{
private readonly IUser _user;
private readonly IResourceRegistry _resourceRegistry;

public UserService(
    IUser user,
    IResourceRegistry resourceRegistry)
{
    _user = user ?? throw new ArgumentNullException(nameof(user));
    _resourceRegistry = resourceRegistry ?? throw new ArgumentNullException(nameof(resourceRegistry));
}

public async Task<bool> CurrentUserIsOwner(string serviceResource, CancellationToken cancellationToken)
{
    var resourceIds = await GetCurrentUserResourceIds(cancellationToken);
    return resourceIds.Contains(serviceResource);
}

public Task<IReadOnlyCollection<string>> GetCurrentUserResourceIds(CancellationToken cancellationToken)
{
    if (!_user.TryGetOrgNumber(out var orgNumber))
    {
        throw new UnreachableException();
    }

    return _resourceRegistry.GetResourceIds(orgNumber, cancellationToken);
}
}

internal sealed class LocalDevelopmentUserServiceDecorator : IUserService
{
private readonly IUserService _userService;

public LocalDevelopmentUserServiceDecorator(IUserService userService)
{
    _userService = userService ?? throw new ArgumentNullException(nameof(userService));
}

public Task<bool> CurrentUserIsOwner(string serviceResource, CancellationToken cancellationToken) => 
    Task.FromResult(true);

public Task<IReadOnlyCollection<string>> GetCurrentUserResourceIds(CancellationToken cancellationToken) => 
    _userService.GetCurrentUserResourceIds(cancellationToken);
}
