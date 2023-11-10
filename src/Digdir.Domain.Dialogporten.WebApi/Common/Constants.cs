namespace Digdir.Domain.Dialogporten.WebApi.Common;

internal static class Constants
{
    internal const string IfMatch = "If-Match";

    internal const string SummaryError400 =
        "Validation error occured. See problem details for a list of errors.";

    internal const string SummaryError422 =
        "Domain error occured. See problem details for a list of errors.";

    internal const string SummaryErrorServiceOwner401 =
        "Missing or invalid authentication token. Requires a Maskinporten-token with the scope \"digdir:dialogporten.serviceprovider\"";

}
