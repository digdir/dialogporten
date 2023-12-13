using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace Digdir.Domain.Dialogporten.WebApi.Common.Authentication;

public class JwtSchemeSelectorMiddleware
{
    private readonly RequestDelegate _next;

    public JwtSchemeSelectorMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(Constants.Authorization, out var authorizationHeader))
        {
            var token = authorizationHeader.ToString().Split(' ').LastOrDefault();

            if (!string.IsNullOrEmpty(token))
            {
                var handler = new JwtSecurityTokenHandler();
                if (handler.CanReadToken(token))
                {
                    var jwtToken = handler.ReadJwtToken(token);
                    context.Items[Constants.CurrentTokenIssuer] = jwtToken.Issuer;
                }
            }
        }

        await _next(context);
    }
}
