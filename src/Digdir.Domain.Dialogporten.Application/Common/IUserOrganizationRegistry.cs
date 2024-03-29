using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;

namespace Digdir.Domain.Dialogporten.Application.Common;

public interface IUserOrganizationRegistry
{
    Task<string?> GetCurrentUserOrgShortName(CancellationToken cancellationToken);
}

public class UserOrganizationRegistry : IUserOrganizationRegistry
{
    private readonly IUser _user;
    private readonly IOrganizationRegistry _organizationRegistry;

    public UserOrganizationRegistry(IUser user, IOrganizationRegistry organizationRegistry)
    {
        _user = user ?? throw new ArgumentNullException(nameof(user));
        _organizationRegistry = organizationRegistry ?? throw new ArgumentNullException(nameof(organizationRegistry));
    }

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

internal sealed class LocalDevelopmentUserOrganizationRegistryDecorator : IUserOrganizationRegistry
{
    private readonly IUserOrganizationRegistry _userOrganizationRegistry;

    public LocalDevelopmentUserOrganizationRegistryDecorator(IUserOrganizationRegistry userOrganizationRegistry)
    {
        _userOrganizationRegistry = userOrganizationRegistry ?? throw new ArgumentNullException(nameof(userOrganizationRegistry));
    }

    public Task<string?> GetCurrentUserOrgShortName(CancellationToken cancellationToken) => Task.FromResult("digdir")!;
}
