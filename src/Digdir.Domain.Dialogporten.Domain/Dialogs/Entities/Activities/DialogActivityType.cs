using Digdir.Library.Entity.Abstractions.Features.Lookup;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;

public class DialogActivityType : AbstractLookupEntity<DialogActivityType, DialogActivityType.Values>
{
    public DialogActivityType(Values id) : base(id) { }
    public override DialogActivityType MapValue(Values id) => new(id);

    public enum Values
    {
        /// <summary>
        /// Refererer en dialog som er opprettet.
        /// </summary>
        DialogCreated = 1,

        /// <summary>
        /// Refererer en dialog som er fullført.
        /// </summary>
        DialogCompleted = 2,

        /// <summary>
        /// Refererer en forsendelse som er åpnet.
        /// </summary>
        TransmissionOpened = 3,

        /// <summary>
        /// TODO: (?) eksempel: Startet utfylling av oppgaver.
        /// </summary>
        Information = 4,

        /// <summary>
        /// Indikerer at betaling har blitt utført.
        /// </summary>
        PaymentMade = 5,

        /// <summary>
        /// Indikerer at signatur er utført.
        /// </summary>
        SignatureProvided = 7
    }
}
