using Digdir.Domain.Dialogporten.Infrastructure;
using Digdir.Domain.Dialogporten.Infrastructure.Common.Exceptions;
using FluentAssertions;
using NetArchTest.Rules;

namespace Digdir.Domain.Dialogporten.Architecture.Tests;

public class InfrastructureArchitectureTests
{
    [Fact]
    public void All_Classes_In_Infrastructure_Should_Be_Internal()
    {
        var publicByDesignClasses = new[]
        {
            nameof(InfrastructureAssemblyMarker),
            nameof(InfrastructureExtensions),

            // These classes are currently public but should be internal, moved to another assembly, or deleted
            nameof(IUpstreamServiceError)
        };

        var publicClasses = Types
            .InAssembly(InfrastructureAssemblyMarker.Assembly)
            .That().DoNotResideInNamespaceMatching("Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations")
            .And().DoNotHaveNameEndingWith("Settings")
            .And().DoNotHaveNameEndingWith("Constants")
            .And().AreNotInterfaces()
            .And().DoNotHaveName(publicByDesignClasses)
            .Should().NotBePublic()
            .GetResult();

        publicClasses.FailingTypes.Should().BeNullOrEmpty();
        publicClasses.IsSuccessful.Should().BeTrue();
    }
}
