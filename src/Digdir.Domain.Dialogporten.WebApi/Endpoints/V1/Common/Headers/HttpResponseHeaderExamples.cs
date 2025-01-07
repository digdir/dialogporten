using Digdir.Domain.Dialogporten.WebApi.Common;
using FastEndpoints;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Common.Headers;

public static class HttpResponseHeaderExamples
{
    public static ResponseHeader NewDialogETagHeader(int statusCode)
        => new(statusCode, Constants.ETag)
        {
            Description = "The new UUID ETag of the dialog",
        };
}
