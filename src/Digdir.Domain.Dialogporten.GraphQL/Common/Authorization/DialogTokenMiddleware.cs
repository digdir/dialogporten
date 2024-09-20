using System.Security.Claims;
using Digdir.Domain.Dialogporten.Application.Common;

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
        if (!_compactJwsGenerator.VerifyCompactJws(token ?? string.Empty))
        {
            return _next(context);
        }

        if (!_compactJwsGenerator.VerifyCompactJwsTimestamp(token!))
        {
            return _next(context);
        }

        if (!_compactJwsGenerator.TryGetClaimValue(dialogToken!, DialogTokenClaimTypes.DialogId, out var dialogTokenDialogId))
        {
            return _next(context);
        }

        context.User.AddIdentity(new ClaimsIdentity([new Claim("dialogId", dialogTokenDialogId)]));

        return _next(context);
    }
}
