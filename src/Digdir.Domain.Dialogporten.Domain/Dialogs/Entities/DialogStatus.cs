using Digdir.Library.Entity.Abstractions.Features.Lookup;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

public class DialogStatus : AbstractLookupEntity<DialogStatus, DialogStatus.Values>
{
    public DialogStatus(Values id) : base(id) { }
    public override DialogStatus MapValue(Values id) => new(id);

    public enum Values
    {
        /// <summary>
        /// The dialogue is considered new. Typically used for simple messages that do not require any interaction,
        /// or as an initial step for dialogues. This is the default.
        /// </summary>
        New = 1,

        /// <summary>
        /// Started. In a serial process, this is used to indicate that, for example, a form filling is ongoing.
        /// </summary>
        InProgress = 2,

        /// <summary>
        /// Equivalent to "InProgress", but will be used by the workspace/frontend for display purposes.
        /// </summary>
        Signing = 3,

        /// <summary>
        /// For processing by the service owner. In a serial process, this is used after a submission is made.
        /// </summary>
        Processing = 4,

        /// <summary>
        /// Used to indicate that the dialogue is in progress/under work, but is in a state where the user must do something - for example, correct an error, or other conditions that hinder further processing.
        /// </summary>
        RequiresAttention = 5,

        /// <summary>
        /// The dialogue was completed. This typically means that the dialogue is moved to a GUI archive or similar.
        /// </summary>
        Completed = 6
    }
}
