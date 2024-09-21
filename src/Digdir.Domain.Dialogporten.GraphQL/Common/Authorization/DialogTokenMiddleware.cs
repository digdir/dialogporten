using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Digdir.Domain.Dialogporten.Application.Common;
using Microsoft.IdentityModel.Tokens;

namespace Digdir.Domain.Dialogporten.GraphQL.Common.Authorization;

public sealed class DialogTokenMiddleware
{
    public const string DialogTokenHeader = "DigDir-Dialog-Token";
    private readonly RequestDelegate _next;
    private readonly ICompactJwsGenerator _compactJwsGenerator;

    public DialogTokenMiddleware(RequestDelegate next, ICompactJwsGenerator compactJwsGenerator)
    {
        _next = next;
        _compactJwsGenerator = compactJwsGenerator;
    }

    public Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(DialogTokenHeader, out var dialogToken))
        {
            return _next(context);
        }

        var token = dialogToken.FirstOrDefault();
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                // ValidateLifetime = false,
                ValidateIssuerSigningKey = false,
                SignatureValidator = (token, parameters) =>
                {
                    var jwt = new JwtSecurityToken(token);
                    return jwt;
                },
            }, out var securityToken);

            var jwt = securityToken as JwtSecurityToken;
            context.User.AddIdentity(new ClaimsIdentity(jwt!.Claims));

            return _next(context);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return _next(context);
        }
    }
}
