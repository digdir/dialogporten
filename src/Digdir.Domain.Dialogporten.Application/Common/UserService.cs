using System.Diagnostics;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;

namespace Digdir.Domain.Dialogporten.Application.Common;

internal sealed class UserService
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

    public Task<string[]> GetCurrentUserResourceIds(CancellationToken cancellationToken)
    {
        if (!_user.TryGetOrgNumber(out var orgNumber))
        {
            throw new UnreachableException();
        }

        return _resourceRegistry.GetResourceIds(orgNumber, cancellationToken);
    }
}