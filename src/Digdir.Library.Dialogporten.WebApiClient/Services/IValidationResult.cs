namespace Altinn.ApiClients.Dialogporten.Services;

public interface IValidationResult
{
    bool IsValid { get; }
    Dictionary<string, List<string>> Errors { get; }
}

internal sealed class DefaultValidationResult : IValidationResult
{
    public Dictionary<string, List<string>> Errors { get; } = [];
    public bool IsValid => Errors.Count == 0;

    internal void AddError(string key, string message)
    {
        if (!Errors.TryGetValue(key, out var messages))
        {
            Errors[key] = messages = [];
        }

        messages.Add(message);
    }
}
