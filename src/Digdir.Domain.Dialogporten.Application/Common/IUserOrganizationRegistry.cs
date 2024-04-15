using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;

namespace Digdir.Domain.Dialogporten.Application.Common;

public interface IUserOrganizationRegistry
{
    Task<string?> GetCurrentUserOrgShortName(CancellationToken cancellationToken);
    Task<IList<OrganizationLongName>?> GetCurrentUserOrgLongNames(CancellationToken cancellationToken);
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

        var orgInfo = await _organizationRegistry.GetOrgInfo(orgNumber, cancellationToken);

        return orgInfo?.ShortName;
    }

    public async Task<IList<OrganizationLongName>?> GetCurrentUserOrgLongNames(CancellationToken cancellationToken)
    {
        if (!_user.TryGetOrgNumber(out var orgNumber))
        {
            return null;
        }

        var orgInfo = await _organizationRegistry.GetOrgInfo(orgNumber, cancellationToken);

        return orgInfo?.LongNames.ToArray();
    }
}

internal sealed class LocalDevelopmentUserOrganizationRegistryDecorator : IUserOrganizationRegistry
{
    public LocalDevelopmentUserOrganizationRegistryDecorator(IUserOrganizationRegistry _) { }

    public Task<string?> GetCurrentUserOrgShortName(CancellationToken cancellationToken) => Task.FromResult("digdir")!;
    public Task<IList<OrganizationLongName>?> GetCurrentUserOrgLongNames(CancellationToken cancellationToken) =>
        Task.FromResult<IList<OrganizationLongName>?>(new[] { new OrganizationLongName { LongName = "Digdir", Language = "nb" } });
}
