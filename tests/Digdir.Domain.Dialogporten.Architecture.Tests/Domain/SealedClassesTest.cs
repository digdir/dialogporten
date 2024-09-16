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

        var typesWithInheritors = new HashSet<Type>(
            allTypes.Where(t => t.BaseType != null)
                .Select(t => t.BaseType!.IsGenericType ?
                    t.BaseType.GetGenericTypeDefinition() : t.BaseType)
        );

        var nonInheritedTypes = allTypes
            .Where(t => !typesWithInheritors.Contains(t))
            .ToList();

        var failingTypes = nonInheritedTypes
            .Where(t => !t.IsSealed)
            .ToList();

        Assert.Empty(failingTypes);
    }
}
