using Digdir.Library.Entity.Abstractions.Features.Lookup;

namespace Digdir.Domain.Dialogporten.Domain.Dialogues;

public class DialogueStatus : AbstractLookupEntity<DialogueStatus, DialogueStatus.Enum>
{
    public DialogueStatus(Enum id) : base(id) { }
    public override DialogueStatus MapValue(Enum id) => new(id);

    public enum Enum
    {
        /// <summary>
        /// Dialogen har ingen spesiell status. Brukes typisk for enkle meldinger som ikke krever noe 
        /// interaksjon. Dette er default. 
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// Under arbeid. Generell status som brukes for dialogtjenester der ytterligere bruker-input er 
        /// forventet.
        /// </summary>
        UnderProgress = 1,

        /// <summary>
        /// Venter på tilbakemelding fra tjenesteeier
        /// </summary>
        Waiting = 2,

        /// <summary>
        /// Dialogen er i en tilstand hvor den venter på signering. Typisk siste steg etter at all  
        /// utfylling er gjennomført og validert. 
        /// </summary>
        Signing = 3,

        /// <summary>
        /// Dialogen ble avbrutt. Dette gjør at dialogen typisk fjernes fra normale GUI-visninger.
        /// </summary>
        Cancelled = 4,

        /// <summary>
        /// Dialigen ble fullført. Dette gjør at dialogen typisk flyttes til et GUI-arkiv eller lignende.
        /// </summary>
        Completed = 5
    }
}
