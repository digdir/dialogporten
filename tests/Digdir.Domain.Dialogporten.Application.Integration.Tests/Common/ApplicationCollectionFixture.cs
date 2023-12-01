namespace Digdir.Domain.Dialogporten.Application.Integration.Tests.Common;

public abstract class ApplicationCollectionFixture : IAsyncLifetime
{
    protected DialogApplication Application { get; }

    protected ApplicationCollectionFixture(DialogApplication application)
    {
        Application = application;
    }

    public Task DisposeAsync() => Task.CompletedTask;
    public Task InitializeAsync() => Application.ResetState();
}

//[CollectionDefinition(nameof(DialogApplicationFixture))]
//public class DialogApplicationFixture : ICollectionFixture<DialogApplication>
//{
//    // This class has no code, and is never created. Its purpose is simply
//    // to be the place to apply [CollectionDefinition] and all the
//    // ICollectionFixture<> interfaces.
//}
