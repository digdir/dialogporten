using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

        services.AddSingleton<ITokenIssuerCache, TokenIssuerCache>();

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

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = async context =>
                    {
                        var issuerCache = context.HttpContext.RequestServices.GetRequiredService<ITokenIssuerCache>();
                        var expectedIssuer = await issuerCache.GetIssuerForScheme(schema.Name);
                        if (context.HttpContext.Items.TryGetValue(Constants.CurrentTokenIssuer, out var issuerObject))
                        {
                            var actualIssuer = issuerObject as string;
                            if (actualIssuer != expectedIssuer)
                            {
                                context.NoResult();
                                return;
                            }
                        }
                    }
                };
            });
        }

        return services;
    }
}
