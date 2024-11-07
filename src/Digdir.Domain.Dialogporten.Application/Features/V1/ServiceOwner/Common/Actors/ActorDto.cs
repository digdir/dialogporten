using Digdir.Domain.Dialogporten.Domain.Actors;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Common.Actors;

public sealed class ActorDto
{
    /// <summary>
    /// The type of actor that sent the transmission.
    /// </summary>
    public ActorType.Values ActorType { get; set; }

    /// <summary>
    /// Specifies the name of the entity that sent the transmission. Mutually exclusive with ActorId. If ActorId
    /// is supplied, the name will be automatically populated from the name registries.
    /// </summary>
    /// <example>Ola Nordmann</example>
    public string? ActorName { get; set; }

    /// <summary>
    /// The identifier of the person or organization that sent the transmission. Mutually exclusive with ActorName.
    /// Might be omitted if ActorType is "ServiceOwner".
    /// </summary>
    /// <example>urn:altinn:person:identifier-no:12018212345</example>
    public string? ActorId { get; set; }
}
