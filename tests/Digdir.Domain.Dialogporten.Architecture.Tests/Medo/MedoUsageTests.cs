using NetArchTest.Rules;
using Medo;
using IdentifiableExtensions = Digdir.Library.Entity.Abstractions.Features.Identifiable.IdentifiableExtensions;

namespace Digdir.Domain.Dialogporten.Architecture.Tests.Medo;

public class MedoUsageTests
{
    [Fact]
    public void Medo_Namespace_Should_Only_Be_Used_In_IdentifiableExtensions()
    {
        var identifiableExtensionsNamespace = typeof(IdentifiableExtensions).Namespace;
        var medoNamespace = typeof(Uuid7).Namespace;

        var result = Types
            .InAssemblies(DialogportenAssemblies.All)
            .That()
            .DoNotResideInNamespace(identifiableExtensionsNamespace)
            .ShouldNot()
            .HaveDependencyOn(medoNamespace)
            .GetResult();

        var failingTypes = result.FailingTypes ?? new List<Type>();

        Assert.Empty(failingTypes);
        Assert.True(result.IsSuccessful);
    }
}
