using Digdir.Library.Entity.Abstractions.Features.Lookup;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

public class ActorType : AbstractLookupEntity<ActorType, ActorType.Values>
{
    public ActorType(Values id) : base(id) { }
    public override ActorType MapValue(Values id) => new(id);

    public enum Values
    {
        /// <summary>
        /// Unknown user type (undeterminable)
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// ID-porten authenticated (has "pid" claim)
        /// </summary>
        Person = 1,

        /// <summary>
        /// Altinn 2 legacy system user ("virksomhetsbruker")
        /// </summary>
        LegacySystemUser = 2,

        /// <summary>
        /// Altinn 3 system user
        /// </summary>
        SystemUser = 3,

        /// <summary>
        /// Maskinporten authenticated service owner
        /// </summary>
        ServiceOwner = 4,

        /// <summary>
        /// Like "Person", but via a service owner system
        /// </summary>
        UserViaServiceOwner = 5
    }
}
