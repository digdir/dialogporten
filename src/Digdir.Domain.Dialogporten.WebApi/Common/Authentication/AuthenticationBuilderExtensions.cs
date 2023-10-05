using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;

namespace Digdir.Domain.Dialogporten.WebApi.Common.Authentication;

internal static class AuthenticationBuilderExtensions
{
    public static IServiceCollection AddDialogportenAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtTokenSchemas = configuration
            .GetSection(WebApiSettings.SectionName)
            .Get<WebApiSettings>()
            ?.Authentication
            ?.JwtBearerTokenSchemas;

        if (jwtTokenSchemas is null || jwtTokenSchemas.Count == 0)
        {
            // Validation should have caught this.
            throw new UnreachableException();
        }

        var authenticationBuilder = services.AddAuthentication();

        foreach (var schema in jwtTokenSchemas)
        {
            authenticationBuilder.AddJwtBearer(schema.Name, options =>
            {
                options.MetadataAddress = schema.WellKnown;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
        }

        return services;
    }
}