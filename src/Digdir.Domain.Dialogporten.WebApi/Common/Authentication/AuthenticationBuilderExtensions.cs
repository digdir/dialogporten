using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Diagnostics;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using Microsoft.AspNetCore.Authentication;

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
                    ClockSkew = TimeSpan.FromSeconds(2)
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = OnMessageReceived(schema)
                };
            });
        }

        return services;
    }

    private static Func<MessageReceivedContext, Task> OnMessageReceived(JwtBearerTokenSchemasOptions schema) =>
        async context =>
        {
            string? expectedIssuer;

            try
            {
                expectedIssuer = await context.HttpContext
                    .RequestServices
                    .GetRequiredService<ITokenIssuerCache>()
                    .GetIssuerForScheme(schema.Name);
            }
            catch (Exception)
            {
                var problemDetails = context.HttpContext.ResponseBuilder(StatusCodes.Status502BadGateway);

                // context.Response.OnStarting(async () =>
                // {
                //     context.Response.StatusCode = StatusCodes.Status502BadGateway;
                //     await context.Response.WriteAsJsonAsync(problemDetails);
                // });
                context.Response.StatusCode = StatusCodes.Status502BadGateway;
                await context.Response.WriteAsJsonAsync(problemDetails);
                return;
            }


            if (context.HttpContext.Items.TryGetValue(Constants.CurrentTokenIssuer, out var tokenIssuer)
                && (string?)tokenIssuer != expectedIssuer)
            {
                context.NoResult();
            }
        };
}
