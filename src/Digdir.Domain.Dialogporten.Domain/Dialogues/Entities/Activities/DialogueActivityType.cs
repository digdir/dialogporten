using Digdir.Library.Entity.Abstractions.Features.Lookup;

namespace Digdir.Domain.Dialogporten.Domain.Dialogues.Entities.Activities;

public class DialogueActivityType : AbstractLookupEntity<DialogueActivityType, DialogueActivityType.Enum>
{
    public DialogueActivityType(Enum id) : base(id) { }
    public override DialogueActivityType MapValue(Enum id) => new(id);

    public enum Enum
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
        /// Når dialogen først ble hentet og av hvem. Kan brukes for å avgjøre om purring 
        /// skal sendes ut, eller internt i virksomheten for å tracke tilganger/bruker.
        /// Merk at dette ikke er det samme som "lest", dette må tjenestetilbyder selv håndtere 
        /// i egne løsninger.
        /// </summary>
        Seen = 6,

        /// <summary>
        /// Når dialogen blir videresendt (tilgang delegert) av noen med tilgang til andre.
        /// </summary>
        Forwarded = 7,
    }
}
