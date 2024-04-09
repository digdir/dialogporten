using Digdir.Library.Entity.Abstractions.Features.Lookup;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;

public class DialogActivityType : AbstractLookupEntity<DialogActivityType, DialogActivityType.Values>
{
    public DialogActivityType(Values id) : base(id) { }
    public override DialogActivityType MapValue(Values id) => new(id);

    public enum Values
    {
        /// <summary>
        /// Refererer en innsending utført av party som er mottatt hos tjenestetilbyder.
        /// </summary>
        Submission = 1,

        /// <summary>
        /// Indikerer en tilbakemelding fra tjenestetilbyder på en innsending. Inneholder
        /// referanse til den aktuelle innsendingen.
        /// </summary>
        Feedback = 2,

        /// <summary>
        /// Informasjon fra tjenestetilbyder, ikke (direkte) relatert til noen innsending.
        /// </summary>
        Information = 3,

        /// <summary>
        /// Brukes for å indikere en feilsituasjon, typisk på en innsending. Inneholder en
        /// tjenestespesifikk activityErrorCode.
        /// </summary>
        Error = 4,

        /// <summary>
        /// Indikerer at dialogen er lukket for videre endring. Dette skjer typisk ved fullføring
        /// av dialogen, eller sletting.
        /// </summary>
        Closed = 5,

        /// <summary>
        /// Når dialogen blir videresendt (tilgang delegert) av noen med tilgang til andre.
        /// </summary>
        Forwarded = 7
    }
}
