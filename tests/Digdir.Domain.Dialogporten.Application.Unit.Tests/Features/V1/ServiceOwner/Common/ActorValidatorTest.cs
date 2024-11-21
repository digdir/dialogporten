using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Common.Actors;
using Digdir.Domain.Dialogporten.Domain.Actors;
using Digdir.Tool.Dialogporten.GenerateFakeData;

namespace Digdir.Domain.Dialogporten.Application.Unit.Tests.Features.V1.ServiceOwner.Common;

public class ActorValidatorTest
{
    private readonly ActorValidator _actorValidator = new();

    [Fact]
    public void Given_Invalid_ActorId_Should_Return_Error()
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

    [Theory]
    [InlineData(ActorType.Values.PartyRepresentative)]
    [InlineData(ActorType.Values.ServiceOwner)]
    public void Given_Valid_ActorId_Should_Return_Success(ActorType.Values actorType)
    {

        var actorDto = new ActorDto
        {
            ActorType = actorType,
            ActorId = actorType == ActorType.Values.PartyRepresentative ? DialogGenerator.GenerateRandomParty(forcePerson: true) : null
        };
        var result = _actorValidator.Validate(actorDto);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Given_Null_ActorType_Should_Return_Error()
    {
        var actorDto = new ActorDto
        {
            ActorId = DialogGenerator.GenerateRandomParty(forcePerson: true)
        };
        var result = _actorValidator.Validate(actorDto);
        Assert.NotEmpty(result.Errors);
    }


    [Theory]
    [InlineData(ActorType.Values.PartyRepresentative)]
    [InlineData(ActorType.Values.ServiceOwner)]
    public void Given_ActorId_And_ActorName_Should_Return_Error(ActorType.Values actorType)
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
    public void ActorType_ServiceOwner_Rules(string? actorName, bool generateActorId)
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
