using Digdir.Domain.Dialogporten.Application.Integration.Tests.Common;
using Digdir.Domain.Dialogporten.Application.Integration.Tests.Features.V1.Dialogs;
using Xunit;

namespace Digdir.Domain.Dialogporten.Application.Integration.Tests.Features.V1.Dialogs.Queries;

[Collection(nameof(DialogCqrsCollectionFixture))]
public class ListDialogTests : ApplicationCollectionFixture
{
    public ListDialogTests(DialogApplication application) : base(application)
    {
    }
    // TODO: Add tests
}
