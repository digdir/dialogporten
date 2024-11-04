using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Common.Actors;
using Digdir.Domain.Dialogporten.Domain.Actors;
using Digdir.Tool.Dialogporten.GenerateFakeData;

namespace Digdir.Domain.Dialogporten.Application.Unit.Tests.Features.V1.ServiceOwner.Common;

public class ActorValidatorTest
{
    private readonly ActorValidator _actorValidator = new();

    [Fact]
    public void GivenInvalidActorIdShouldReturnError()
    {
        var actorDto = new ActorDto
        {
            ActorType = ActorType.Values.PartyRepresentative,
            ActorName = null,
            ActorId = "InvalidId!"
        };
        var result = _actorValidator.Validate(actorDto);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void GivenValidActorIdShouldReturnSuccess()
    {

        var actorDto = new ActorDto
        {
            ActorType = ActorType.Values.PartyRepresentative,
            ActorName = null,
            ActorId = DialogGenerator.GenerateRandomParty(forcePerson: true)
        };
        var result = _actorValidator.Validate(actorDto);
        Assert.Empty(result.Errors);
    }


    [Theory]
    [InlineData(ActorType.Values.PartyRepresentative)]
    [InlineData(ActorType.Values.ServiceOwner)]
    public void GivenActorIdAndActorNameShouldReturnError(ActorType.Values actorType)
    {
        var actorDto = new ActorDto
        {
            ActorType = actorType,
            ActorName = "Fredrik Testland",
            ActorId = DialogGenerator.GenerateRandomParty(forcePerson: true)
        };
        var result = _actorValidator.Validate(actorDto);
        Assert.NotEmpty(result.Errors);
    }

    [Theory]
    [InlineData("Fredik TestLand", false)]
    [InlineData(null, true)]
    public void ActorTypeServiceOwnerRules(string? actorName, bool generateActorId)
    {
        var actorDto = new ActorDto
        {
            ActorType = ActorType.Values.ServiceOwner,
            ActorName = actorName,
            ActorId = generateActorId ? DialogGenerator.GenerateRandomParty(forcePerson: true) : null
        };
        var result = _actorValidator.Validate(actorDto);
        Assert.NotEmpty(result.Errors);
    }
}
