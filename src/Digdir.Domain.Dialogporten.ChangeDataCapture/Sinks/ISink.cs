namespace Digdir.Domain.Dialogporten.ChangeDataCapture.Sinks;

public interface ISink
{
    Task Init(CancellationToken cancellationToken);
    Task Send(object @event, CancellationToken cancellationToken);
}
