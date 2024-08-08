using Digdir.Domain.Dialogporten.Domain.Common;
using Digdir.Library.Entity.Abstractions.Features.Lookup;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Contents;

public class TransmissionContentType : AbstractLookupEntity<TransmissionContentType, TransmissionContentType.Values>
{
    public TransmissionContentType(Values id) : base(id) { }
    public enum Values
    {
        Title = 1,
        Summary = 2
    }

    public bool Required { get; private init; }
    public int MaxLength { get; private init; }

    public string[] AllowedMediaTypes { get; init; } = [];

    public override TransmissionContentType MapValue(Values id) => id switch
    {
        Values.Title => new(id)
        {
            Required = true,
            MaxLength = Constants.DefaultMaxStringLength,
            AllowedMediaTypes = [MediaTypes.PlainText]
        },
        Values.Summary => new(id)
        {
            Required = true,
            MaxLength = Constants.DefaultMaxStringLength,
            AllowedMediaTypes = [MediaTypes.PlainText]
        },
        _ => throw new ArgumentOutOfRangeException(nameof(id), id, null)
    };

    public static readonly Values[] RequiredTypes = GetValues()
        .Where(x => x.Required)
        .Select(x => x.Id)
        .ToArray();
}
