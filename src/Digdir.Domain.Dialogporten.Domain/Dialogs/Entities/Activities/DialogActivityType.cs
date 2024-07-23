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
        /// Refererer en dialog som er lukket.
        /// </summary>
        DialogClosed = 3,

        /// <summary>
        /// Informasjon fra tjenestetilbyder, ikke (direkte) relatert til noen innsending.
        /// </summary>
        Information = 4,

        /// <summary>
        /// Refererer en forsendelse som er åpnet.
        /// </summary>
        TransmissionOpened = 5,

        /// <summary>
        /// Indikerer at betaling har blitt utført.
        /// </summary>
        PaymentMade = 6,

        /// <summary>
        /// Indikerer at signatur er utført.
        /// </summary>
        SignatureProvided = 7
    }
}
