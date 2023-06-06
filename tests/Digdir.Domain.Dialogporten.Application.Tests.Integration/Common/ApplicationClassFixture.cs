using Xunit;

namespace Digdir.Domain.Dialogporten.Application.Tests.Integration.Common;

public abstract class ApplicationClassFixture : ApplicationCollectionFixture, IClassFixture<DialogApplication>
{
    protected ApplicationClassFixture(DialogApplication application) : base(application) { }
}
