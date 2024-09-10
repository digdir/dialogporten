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
        /// Refers to a dialog that has been closed.
        /// </summary>
        DialogClosed = 2,

        /// <summary>
        /// Information from the service provider, not (directly) related to any transmission.
        /// </summary>
        Information = 3,

        /// <summary>
        /// Refers to a transmission that has been opened.
        /// </summary>
        TransmissionOpened = 4,

        /// <summary>
        /// Indicates that payment has been made.
        /// </summary>
        PaymentMade = 5,

        /// <summary>
        /// Indicates that a signature has been provided.
        /// </summary>
        SignatureProvided = 6,

        /// <summary>
        /// Refers to a dialog that has been opened.
        /// </summary>
        DialogOpened = 7,
    }
}
