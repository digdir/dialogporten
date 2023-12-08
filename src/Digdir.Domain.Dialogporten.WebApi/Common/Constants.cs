namespace Digdir.Domain.Dialogporten.WebApi.Common;

internal static class Constants
{
    internal const string IfMatch = "If-Match";

    internal static class SwaggerSummary
    {
        internal const string ReturnedResult = "Successfully returned the dialog {0}.";
        internal const string Created = "The UUID of the created the dialog {0}. A relative URL to the newly created activity is set in the \"Location\" header.";
        internal const string Deleted = "The dialog {0} was deleted successfully.";
        internal const string Updated = "The dialog {0} was updated successfully.";
        internal const string ValidationError = "Validation error occured. See problem details for a list of errors.";
        internal const string DomainError = "Domain error occured. See problem details for a list of errors.";
        internal const string ServiceOwnerAuthenticationFailure = "Missing or invalid authentication token. Requires a Maskinporten-token with the scope \"{1}\".";
        internal const string EndUserAuthenticationFailure = "Missing or invalid authentication token. Requires a Maskinporten-token with the scope \"digdir:dialogporten\".";
        internal const string DialogNotFound = "The given dialog ID was not found or is already deleted.";
        internal const string DialogActivityNotFound = "The given dialog ID was not found or was deleted, or the given activity ID was not found.";
        internal const string DialogElementNotFound = "The given dialog ID or dialog element ID was not found or was already deleted.";
        internal const string EtagMismatch = "The supplied If-Match header did not match the current ETag value for the dialog. The request was not applied.";
        internal const string AccessDeniedToDialog = "Unauthorized to {0} the supplied dialog (not owned by authenticated organization or has additional scope requirements defined in policy).";
        internal const string AccessDeniedToDialogForChildEntity = "Unauthorized to {0} child entity for the given dialog (dialog not owned by authenticated organization or has additional scope requirements defined in service identifiers policy).";
        internal const string DialogCreationNotAllowed = "Unauthorized to create a dialog for the given serviceResource (not owned by authenticated organization or has additional scope requirements defined in policy).";
        internal const string OptimisticConcurrencyNote = "Optimistic concurrency control is implemented using the If-Match header. Supply the ETag value from the GetDialog endpoint to ensure that the dialog is not modified/deleted by another request in the meantime.";
    }
}

