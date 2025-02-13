namespace Altinn.ApiClients.Dialogporten;

public interface IDialogTokenVerifier
{
    bool Verify(ReadOnlySpan<char> token);
}
