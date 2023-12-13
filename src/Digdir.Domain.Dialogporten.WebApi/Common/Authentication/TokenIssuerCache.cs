using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using System.Collections.ObjectModel;

namespace Digdir.Domain.Dialogporten.WebApi.Common.Authentication;

public interface ITokenIssuerCache
{
    public Task<string?> GetIssuerForScheme(string schemeName);
}

public class TokenIssuerCache : ITokenIssuerCache, IDisposable
{
    private readonly Dictionary<string, string> _issuerMappings = new();
    private readonly SemaphoreSlim _initializationSemaphore = new(1, 1);
    private bool _initialized;
    private readonly IEnumerable<JwtBearerTokenSchemasOptions>? _jwtTokenSchemas;

    public TokenIssuerCache(IConfiguration configuration)
    {
        _jwtTokenSchemas = configuration
            .GetSection(WebApiSettings.SectionName)
            .Get<WebApiSettings>()
            ?.Authentication
            ?.JwtBearerTokenSchemas;
    }

    public async Task<string?> GetIssuerForScheme(string schemeName)
    {
        await EnsureInitializedAsync();

        return _issuerMappings.TryGetValue(schemeName, out var issuer)
            ? issuer : null;
    }

    private async Task EnsureInitializedAsync()
    {
        if (!_initialized && _jwtTokenSchemas != null)
        {
            await _initializationSemaphore.WaitAsync();
            try
            {
                if (!_initialized)
                {
                    foreach (var schema in _jwtTokenSchemas)
                    {
                        var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                            schema.WellKnown, new OpenIdConnectConfigurationRetriever());
                        var config = await configManager.GetConfigurationAsync();
                        _issuerMappings[schema.Name] = config.Issuer;
                    }

                    _initialized = true;
                }
            }
            finally
            {
                _initializationSemaphore.Release();
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _initializationSemaphore.Dispose();
        }
    }
}
