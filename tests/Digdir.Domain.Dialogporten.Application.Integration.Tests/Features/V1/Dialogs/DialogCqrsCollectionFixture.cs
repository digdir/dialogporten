using Digdir.Domain.Dialogporten.Application.Integration.Tests.Common;
using Xunit;

namespace Digdir.Domain.Dialogporten.Application.Integration.Tests.Features.V1.Dialogs;

[CollectionDefinition(nameof(DialogCqrsCollectionFixture))]
public class DialogCqrsCollectionFixture : ICollectionFixture<DialogApplication>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
