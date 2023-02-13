namespace Digdir.Domain.Dialogporten.Domain.Dialogues;

public class DialogueDates
{
    // TODO: Skal datoene i et eget objekt? Hvorfor?
    /// <summary>
    /// Hvis oppgitt blir dialogen satt med en frist 
    /// (i Altinn2 er denne bare retningsgivende og har ingen effekt, skal dette fortsette?)
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Mulighet for å skjule/deaktivere en dialog på et eller annet tidspunkt?
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    public DateTime? ReadDate { get; set; }
}
