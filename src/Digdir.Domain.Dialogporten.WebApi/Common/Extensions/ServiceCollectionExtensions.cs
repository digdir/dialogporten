using Digdir.Domain.Dialogporten.WebApi.Common.Options;
using Microsoft.IdentityModel.Tokens;

namespace Digdir.Domain.Dialogporten.WebApi.Common.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDialogportenAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var authOptions = configuration
            .GetSection(AuthenticationOptions.SectionName)
            .Get<AuthenticationOptions>();

        // TODO: Validate configuration? 
        if (authOptions is null)
        {
            throw new Exception();
        }

        if (authOptions.JwtBearerTokenSchemas.Count == 0)
        {
            throw new Exception();
        }

        var authenticationBuilder = services.AddAuthentication();

        foreach (var schema in authOptions.JwtBearerTokenSchemas)
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