namespace Altinn.ApiClients.Dialogporten;

public interface IDialogTokenValidator
{
    IValidationResult Validate(ReadOnlySpan<char> token);
}

public interface IValidationResult
{
    bool IsValid { get; }
    Dictionary<string, List<string>> Errors { get; }
}
