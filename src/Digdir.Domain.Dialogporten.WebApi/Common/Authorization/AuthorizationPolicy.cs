using System.Collections.ObjectModel;

namespace Digdir.Domain.Dialogporten.WebApi.Common.Authorization;

internal static class AuthorizationPolicy
{
    public const string ServiceProvider = "serviceprovider";
    public const string ServiceProviderSearch = "serviceproviderSearch";
    public const string Testing = "testing";
}

internal static class AuthorizationScope
{
    public const string ServiceProvider = "digdir:dialogporten.serviceprovider";
    public const string ServiceProviderSearch = "digdir:dialogporten.serviceprovider.search";
    public const string Testing = "digdir:dialogporten.developer.test";

    internal static readonly Lazy<IReadOnlyCollection<string>> AllScopes = new(GetAll);

    private static ReadOnlyCollection<string> GetAll() =>
        typeof(AuthorizationScope)
            .GetFields()
            .Where(x => x.IsLiteral && !x.IsInitOnly && x.DeclaringType == typeof(string))
            .Select(x => (string)x.GetRawConstantValue()!)
            .ToList()
            .AsReadOnly();
}
