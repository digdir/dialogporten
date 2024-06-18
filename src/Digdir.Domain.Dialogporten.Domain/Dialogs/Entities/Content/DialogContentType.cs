using Digdir.Domain.Dialogporten.Domain.Common;
using Digdir.Library.Entity.Abstractions.Features.Lookup;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Content;

public class DialogContentType : AbstractLookupEntity<DialogContentType, DialogContentType.Values>
{
    public DialogContentType(Values id) : base(id) { }
    public enum Values
    {
        Title = 1,
        SenderName = 2,
        Summary = 3,
        AdditionalInfo = 4,
        ExtendedStatus = 5,
        MainContentReference = 6
    }

    public bool Required { get; private init; }
    public bool OutputInList { get; private init; }
    public int MaxLength { get; private init; }

    // This has to be nullable in order to have empty array as default value
    public List<string>? AllowedMediaTypes { get; init; }

    public override DialogContentType MapValue(Values id) => id switch
    {
        Values.Title => new(id)
        {
            Required = true,
            MaxLength = Constants.DefaultMaxStringLength,
            OutputInList = true,
        },
        Values.SenderName => new(id)
        {
            Required = false,
            MaxLength = Constants.DefaultMaxStringLength,
            OutputInList = true,
        },
        Values.Summary => new(id)
        {
            Required = true,
            MaxLength = Constants.DefaultMaxStringLength,
            OutputInList = true,
        },
        Values.AdditionalInfo => new(id)
        {
            Required = false,
            MaxLength = 1023,
            OutputInList = false,
            AllowedMediaTypes = [MediaTypes.Html, MediaTypes.PlainText, MediaTypes.Markdown]
        },
        Values.ExtendedStatus => new(id)
        {
            Required = false,
            MaxLength = 20,
            OutputInList = true,
        },
        Values.MainContentReference => new(id)
        {
            Required = false,
            MaxLength = 1023,
            OutputInList = false,
            AllowedMediaTypes = [MediaTypes.EmbeddableMarkdown]
        },
        _ => throw new ArgumentOutOfRangeException(nameof(id), id, null)
    };

    public static readonly Values[] RequiredTypes = GetValues()
        .Where(x => x.Required)
        .Select(x => x.Id)
        .ToArray();
}
