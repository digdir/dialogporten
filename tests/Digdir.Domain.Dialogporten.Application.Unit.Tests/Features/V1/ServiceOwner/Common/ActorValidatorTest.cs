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


    [Fact]
    public void GivenActorIdAndActorNameShouldReturnError()
    {
        var actorDto = new ActorDto
        {
            ActorType = ActorType.Values.PartyRepresentative,
            ActorName = "Fredrik Testland",
            ActorId = DialogGenerator.GenerateRandomParty(forcePerson: true)
        };
        var result = _actorValidator.Validate(actorDto);
        Assert.NotEmpty(result.Errors);
    }
}
