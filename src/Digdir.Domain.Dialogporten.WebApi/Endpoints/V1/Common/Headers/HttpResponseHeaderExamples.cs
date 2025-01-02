using FastEndpoints;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Common.Headers;

public static class HttpResponseHeaderExamples
{
    public static ResponseHeader NewDialogRevisionHeader(int statusCode)
        => new(statusCode, HttpResponseHeaders.NewDialogRevision)
        {
            Description = "The new UUID revision of the dialog",
        };
}
