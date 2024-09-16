using Digdir.Library.Entity.Abstractions.Features.Lookup;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions;

public sealed class DialogTransmissionType : AbstractLookupEntity<DialogTransmissionType, DialogTransmissionType.Values>
{
    public DialogTransmissionType(Values id) : base(id) { }
    public override DialogTransmissionType MapValue(Values id) => new(id);

    public enum Values
    {
        /// <summary>
        /// For general information, not related to any submissions
        /// </summary>
        Information = 1,

        /// <summary>
        /// Feedback/receipt accepting a previous submission
        /// </summary>
        Acceptance = 2,

        /// <summary>
        /// Feedback/error message rejecting a previous submission
        /// </summary>
        Rejection = 3,

        /// <summary>
        /// Question/request for more information
        /// </summary>
        Request = 4,

        /// <summary>
        /// Critical information about the process
        /// </summary>
        Alert = 5,

        /// <summary>
        /// Information about a formal decision ("resolution")
        /// </summary>
        Decision = 6,

        /// <summary>
        /// A normal submission of some information/form
        /// </summary>
        Submission = 7,

        /// <summary>
        /// A submission correcting/overriding some previously submitted information
        /// </summary>
        Correction = 8
    }
}
