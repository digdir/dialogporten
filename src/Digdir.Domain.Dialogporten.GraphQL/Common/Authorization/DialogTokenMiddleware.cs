using System.IdentityModel.Tokens.Jwt;
using Digdir.Domain.Dialogporten.Infrastructure.GraphQl;
using Microsoft.IdentityModel.Tokens;

namespace Digdir.Domain.Dialogporten.GraphQL.Common.Authorization;

public class DialogTokenMiddleware
{
    public const string DialogTokenHeader = "DigDir-Dialog-Token";
    private readonly RequestDelegate _next;

    public DialogTokenMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    public Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(DialogTokenHeader, out var dialogToken))
        {
            return _next(context);
        }
        var token = dialogToken.FirstOrDefault();
        if (string.IsNullOrEmpty(token))
        {
            return _next(context);
        }
        var tokenValidator = new JwtSecurityTokenHandler();
        if (!tokenValidator.CanReadToken(token))
        {
            return _next(context);
        }

        var jwtToken = tokenValidator.ReadJwtToken(token);

        // tokenValidator.Vali

        var principal = tokenValidator.ValidateToken(dialogToken, new TokenValidationParameters
        {
            // validate
        }, out var validatedToken);
        // // Check if DialogToken is present in headers
        // if (context.Request.Headers.ContainsKey("DialogToken"))
        // {
        //     var dialogToken = context.Request.Headers["DialogToken"].ToString();
        //
        //     // Validate and decode the token
        //     var handler = new JwtSecurityTokenHandler();
        //     var tokenValidationParameters = new TokenValidationParameters
        //     {
        //         // Add your validation parameters, e.g., issuer, audience, signing keys, etc.
        //     };
        //
        //     try
        //     {
        //         // Validate the token and extract claims
        //         var principal = handler.ValidateToken(dialogToken, tokenValidationParameters, out var validatedToken);
        //         var dialogClaims = principal.Claims;
        //
        //         // Add the DialogToken claims to the current ClaimsPrincipal
        //         var identity = new ClaimsIdentity(dialogClaims, "DialogToken");
        //         context.User.AddIdentity(identity);
        //     }
        //     catch (Exception ex)
        //     {
        //         // Handle invalid token scenario
        //         // Log error or handle accordingly
        //     }
        // }
        //
        // await _next(context);

        return _next(context);
    }
}
