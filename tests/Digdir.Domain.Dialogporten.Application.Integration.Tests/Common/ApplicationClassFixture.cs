namespace Digdir.Domain.Dialogporten.Application.Integration.Tests.Common;

public abstract class ApplicationClassFixture : ApplicationCollectionFixture, IClassFixture<DialogApplication>
{
    protected ApplicationClassFixture(DialogApplication application) : base(application) { }
}
