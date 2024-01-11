using Digdir.Domain.Dialogporten.Application.Externals.Presentation;
using Digdir.Domain.Dialogporten.Application.Externals;
using System.Diagnostics;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;

namespace Digdir.Domain.Dialogporten.Application.Common;

internal interface IUserService
{
    Task<bool> CurrentUserIsOwner(string serviceResource, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<string>> GetCurrentUserResourceIds(CancellationToken cancellationToken);
    Task<string?> GetCurrentUserOrgShortName(CancellationToken cancellationToken);
}

internal sealed class UserService : IUserService
{
    private readonly IUser _user;
    private readonly IResourceRegistry _resourceRegistry;
    private readonly IOrganizationRegistry _organizationRegistry;

    public UserService(
        IUser user,
        IResourceRegistry resourceRegistry,
        IOrganizationRegistry organizationRegistry)
    {
        _user = user ?? throw new ArgumentNullException(nameof(user));
        _resourceRegistry = resourceRegistry ?? throw new ArgumentNullException(nameof(resourceRegistry));
        _organizationRegistry = organizationRegistry;
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

    public async Task<string?> GetCurrentUserOrgShortName(CancellationToken cancellationToken)
    {
        if (_user.TryGetOrgShortName(out var orgShortName))
        {
            return orgShortName;
        }

        if (!_user.TryGetOrgNumber(out var orgNumber))
        {
            return null;
        }

        return await _organizationRegistry.GetOrgShortName(orgNumber, cancellationToken);
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

    public Task<string?> GetCurrentUserOrgShortName(CancellationToken cancellationToken) =>
        _userService.GetCurrentUserOrgShortName(cancellationToken);
}
