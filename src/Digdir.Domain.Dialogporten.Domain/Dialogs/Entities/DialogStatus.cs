using Digdir.Library.Entity.Abstractions.Features.Lookup;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

public class DialogStatus : AbstractLookupEntity<DialogStatus, DialogStatus.Values>
{
    public DialogStatus(Values id) : base(id) { }
    public override DialogStatus MapValue(Values id) => new(id);

    public enum Values
    {
        /// <summary>
        /// Dialogen er å regne som ny. Brukes typisk for enkle meldinger som ikke krever noe
        /// interaksjon, eller som et initielt steg for dialoger. Dette er default.
        /// </summary>
        New = 1,

        /// <summary>
        /// Under arbeid. Generell status som brukes for dialogtjenester der ytterligere bruker-input er
        /// forventet.
        /// </summary>
        InProgress = 2,

        /// <summary>
        /// Venter på tilbakemelding fra tjenesteeier
        /// </summary>
        Waiting = 3,

        /// <summary>
        /// Dialogen er i en tilstand hvor den venter på signering. Typisk siste steg etter at all
        /// utfylling er gjennomført og validert.
        /// </summary>
        Signing = 4,

        /// <summary>
        /// Dialogen ble avbrutt. Dette gjør at dialogen typisk fjernes fra normale GUI-visninger.
        /// </summary>
        Cancelled = 5,

        /// <summary>
        /// Dialigen ble fullført. Dette gjør at dialogen typisk flyttes til et GUI-arkiv eller lignende.
        /// </summary>
        Completed = 6
    }
}
