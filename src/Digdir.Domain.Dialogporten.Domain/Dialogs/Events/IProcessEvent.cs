namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Events;

public interface IProcessEvent
{
    string? Process { get; }
    string? PrecedingProcess { get; }
}
