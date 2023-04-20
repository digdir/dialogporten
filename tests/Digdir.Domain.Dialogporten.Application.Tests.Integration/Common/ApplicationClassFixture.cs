using Xunit;

namespace Digdir.Domain.Dialogporten.Application.Tests.Integration.Common;

public abstract class ApplicationClassFixture : ApplicationCollectionFixture, IClassFixture<DialogueApplication>
{
    protected ApplicationClassFixture(DialogueApplication application) : base(application) { }
}
