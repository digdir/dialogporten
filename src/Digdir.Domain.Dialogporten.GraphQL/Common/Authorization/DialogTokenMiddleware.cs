using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Digdir.Domain.Dialogporten.Application;
using Digdir.Domain.Dialogporten.Application.Common;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSec.Cryptography;
using static Digdir.Domain.Dialogporten.Application.Features.V1.Common.Authorization.Constants;

namespace Digdir.Domain.Dialogporten.GraphQL.Common.Authorization;

public sealed class DialogTokenMiddleware
{
    public const string DialogTokenHeader = "DigDir-Dialog-Token";
    private readonly RequestDelegate _next;
    private readonly PublicKey _publicKey;
    private readonly string _issuer;

    public DialogTokenMiddleware(RequestDelegate next, IOptions<ApplicationSettings> applicationSettings)
    {
        _next = next;

        var keyPair = applicationSettings.Value.Dialogporten.Ed25519KeyPairs.Primary;
        _publicKey = PublicKey.Import(SignatureAlgorithm.Ed25519,
            Base64Url.Decode(keyPair.PublicComponent), KeyBlobFormat.RawPublicKey);
        _issuer = applicationSettings.Value.Dialogporten.BaseUri.AbsoluteUri.TrimEnd('/') + DialogTokenIssuerVersion;
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
                ValidateAudience = false,
                ValidIssuer = _issuer,
                SignatureValidator = ValidateSignature
            }, out var securityToken);

            if (securityToken is not JwtSecurityToken jwt)
            {
                return _next(context);
            }

            var dialogIdClaim = jwt.Claims.FirstOrDefault(x => x.Type == DialogTokenClaimTypes.DialogId);
            if (dialogIdClaim is null)
            {
                return _next(context);
            }

            context.User.AddIdentity(new ClaimsIdentity([dialogIdClaim]));

            return _next(context);
        }
        catch (Exception)
        {
            return _next(context);
        }
    }

    private JwtSecurityToken ValidateSignature(string encodedToken, object _)
    {
        var jwt = new JwtSecurityToken(encodedToken);

        var signature = Base64Url.Decode(jwt.RawSignature);
        var signatureIsValid = SignatureAlgorithm.Ed25519
            .Verify(_publicKey, Encoding.UTF8.GetBytes(jwt.EncodedHeader + '.' + jwt.EncodedPayload), signature);

        if (signatureIsValid)
        {
            return jwt;
        }

        throw new SecurityTokenInvalidSignatureException("Invalid token signature.");
    }
}
