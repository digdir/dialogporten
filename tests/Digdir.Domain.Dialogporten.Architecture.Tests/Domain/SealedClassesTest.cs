using NetArchTest.Rules;

namespace Digdir.Domain.Dialogporten.Architecture.Tests.Domain;

public class SealedClassesTest
{
    [Fact]
    public void All_Classes_Without_Inheritors_Should_Be_Sealed()
    {
        var allTypes = Types
            .InAssemblies(DialogportenAssemblies.All)
            .That()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .GetTypes()
            .Where(x => x.Name != nameof(Program))
            .ToList();

        var nonInheritedTypes = allTypes
            .Where(t => !allTypes.Any(other =>
                other.BaseType != null &&
                (other.BaseType == t ||
                (other.BaseType.IsGenericType && other.BaseType.GetGenericTypeDefinition() == t))))
            .ToList();

        var failingTypes = nonInheritedTypes
            .Where(t => !t.IsSealed)
            .ToList();

        Assert.Empty(failingTypes);
    }
}
