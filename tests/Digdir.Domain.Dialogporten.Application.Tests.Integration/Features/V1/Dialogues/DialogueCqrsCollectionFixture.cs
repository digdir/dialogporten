using Digdir.Domain.Dialogporten.Application.Tests.Integration.Common;
using Xunit;

namespace Digdir.Domain.Dialogporten.Application.Tests.Integration.Features.V1.Dialogues;

[CollectionDefinition(nameof(DialogueCqrsCollectionFixture))]
public class DialogueCqrsCollectionFixture : ICollectionFixture<DialogueApplication>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}