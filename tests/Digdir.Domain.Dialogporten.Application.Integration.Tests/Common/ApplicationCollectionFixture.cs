namespace Digdir.Domain.Dialogporten.Application.Integration.Tests.Common;

public abstract class ApplicationCollectionFixture(DialogApplication application) : IAsyncLifetime
{
    protected DialogApplication Application { get; } = application;

    public Task DisposeAsync() => Task.CompletedTask;
    public Task InitializeAsync() => Application.ResetState();
}
