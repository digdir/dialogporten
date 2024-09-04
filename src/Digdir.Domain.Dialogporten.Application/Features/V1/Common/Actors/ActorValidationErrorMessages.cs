namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.Actors;

public static class ActorValidationErrorMessages
{
    public const string ActorIdActorNameExclusiveOr = "If 'ActorType' is 'ServiceOwner', both 'ActorId' and 'ActorName' must be null. " +
                                                      "For any other value of 'ActorType', 'ActorId' or 'ActorName' must be set, but not both simultaneously.";
}
