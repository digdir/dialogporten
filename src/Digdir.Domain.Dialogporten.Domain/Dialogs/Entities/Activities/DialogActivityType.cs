using Digdir.Library.Entity.Abstractions.Features.Lookup;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;

public class DialogActivityType : AbstractLookupEntity<DialogActivityType, DialogActivityType.Values>
{
    public DialogActivityType(Values id) : base(id) { }
    public override DialogActivityType MapValue(Values id) => new(id);

    public enum Values
    {
        /// <summary>
        /// Refers to a dialog that has been created.
        /// </summary>
        DialogCreated = 1,

        /// <summary>
        /// Refers to a dialog that has been completed.
        /// </summary>
        DialogCompleted = 2,

        /// <summary>
        /// Refers to a dialog that has been closed.
        /// </summary>
        DialogClosed = 3,

        /// <summary>
        /// Information from the service provider, not (directly) related to any transmission.
        /// </summary>
        Information = 4,

        /// <summary>
        /// Refers to a transmission that has been opened.
        /// </summary>
        TransmissionOpened = 5,

        /// <summary>
        /// Indicates that payment has been made.
        /// </summary>
        PaymentMade = 6,

        /// <summary>
        /// Indicates that a signature has been provided.
        /// </summary>
        SignatureProvided = 7
    }
}
