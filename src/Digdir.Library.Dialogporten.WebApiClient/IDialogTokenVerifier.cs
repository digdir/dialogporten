using Altinn.ApiClients.Dialogporten.Services;

namespace Altinn.ApiClients.Dialogporten;

public interface IDialogTokenVerifier
{
    IValidationResult Validate(ReadOnlySpan<char> token);
}
