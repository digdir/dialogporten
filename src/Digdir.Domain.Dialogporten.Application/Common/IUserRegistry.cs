using System.Diagnostics;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;
using UserIdType = Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogUserType.Values;

namespace Digdir.Domain.Dialogporten.Application.Common;

public interface IUserRegistry
{
    UserId GetCurrentUserId();
    Task<UserInformation> GetCurrentUserInformation(CancellationToken cancellationToken);
}

public sealed class UserId
{
    public required UserIdType Type { get; set; }
    public required string ExternalId { get; init; }
}

public sealed class UserInformation
{
    public required UserId UserId { get; init; }
    public required IEnumerable<LocalizedName> LocalizedNames { get; init; }
    public string? Name => LocalizedNames.FirstOrDefault()?.Name;
    public string? ServiceOwnerShortName { get; init; }
}

public sealed class LocalizedName
{
    public required string? Name { get; init; }
    public string? LanguageCode { get; init; }
};

public class UserRegistry : IUserRegistry
{
    private readonly IUser _user;
    private readonly IPartyNameRegistry _partyNameRegistry;
    private readonly IServiceOwnerNameRegistry _serviceOwnerNameRegistry;

    public UserRegistry(
        IUser user,
        IPartyNameRegistry partyNameRegistry,
        IServiceOwnerNameRegistry serviceOwnerNameRegistry)
    {
        _user = user ?? throw new ArgumentNullException(nameof(user));
        _partyNameRegistry = partyNameRegistry ?? throw new ArgumentNullException(nameof(partyNameRegistry));
        _serviceOwnerNameRegistry = serviceOwnerNameRegistry ?? throw new ArgumentNullException(nameof(serviceOwnerNameRegistry));
    }

    public UserId GetCurrentUserId()
    {
        var (userType, externalId) = _user.GetPrincipal().GetUserType();
        if (userType == UserIdType.Unknown)
        {
            throw new InvalidOperationException("User external id not found");
        }

        return new() { Type = userType, ExternalId = externalId };
    }

    public async Task<UserInformation> GetCurrentUserInformation(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var names = new List<LocalizedName>();
        string? serviceOwnerShortName = null;
        switch (userId.Type)
        {
            case UserIdType.Person:
            case UserIdType.ServiceOwnerOnBehalfOfPerson:
                names.Add(new() { Name = await _partyNameRegistry.GetPersonName(userId.ExternalId, cancellationToken) });
                break;
            case UserIdType.LegacySystemUser:
                _user.TryGetLegacySystemUserName(out var name);
                names.Add(new() { Name = name });
                break;
            case UserIdType.Enterprise:
                // We might have a request without an "org"-claim, which will then be identified as just another enterprise
                // We therefor will have to check if the user is a service owner by seeing if we can get the service owner short name
                // If not, we fall back to just getting the organization name
                serviceOwnerShortName = await _serviceOwnerNameRegistry.GetServiceOwnerShortName(userId.ExternalId, cancellationToken);
                if (serviceOwnerShortName != null)
                {
                    userId.Type = UserIdType.ServiceOwner;
                    names = await _serviceOwnerNameRegistry.GetServiceOwnerLongNames(userId.ExternalId, cancellationToken);
                }
                else
                {
                    names.Add(new() { Name = await _partyNameRegistry.GetOrganizationName(userId.ExternalId, cancellationToken) });
                }
                break;
            case UserIdType.ServiceOwner:
                names = await _serviceOwnerNameRegistry.GetServiceOwnerLongNames(userId.ExternalId, cancellationToken);
                serviceOwnerShortName = await _serviceOwnerNameRegistry.GetServiceOwnerShortName(userId.ExternalId, cancellationToken);
                break;
            case UserIdType.SystemUser:
                names.Add(new() { Name = "System User" });
                break;
            case UserIdType.Unknown:
            default:
                throw new UnreachableException();
        }

        return new()
        {
            UserId = userId,
            LocalizedNames = names,
            ServiceOwnerShortName = serviceOwnerShortName
        };
    }
}

internal sealed class LocalDevelopmentUserRegistryDecorator : IUserRegistry
{
    private const string LocalDevelopmentUserName = "Local Development User";
    private readonly IUserRegistry _userRegistry;

    public LocalDevelopmentUserRegistryDecorator(IUserRegistry userRegistry)
    {
        _userRegistry = userRegistry ?? throw new ArgumentNullException(nameof(userRegistry));
    }

    public UserId GetCurrentUserId() => _userRegistry.GetCurrentUserId();

    public Task<UserInformation> GetCurrentUserInformation(CancellationToken cancellationToken)
        => Task.FromResult(new UserInformation
        {
            UserId = GetCurrentUserId(),
            // TODO: Decorating service owner org.name and enduser pid is now done in the same decorator
            ServiceOwnerShortName = "digdir",
            LocalizedNames = new List<LocalizedName> { new() { Name = LocalDevelopmentUserName } }
        });
}
