using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.ValueConverters;

internal sealed class DateTimeOffsetConverter : ValueConverter<DateTimeOffset, DateTimeOffset>
{
    public DateTimeOffsetConverter()
        : base(
            d => d.ToUniversalTime(),
            d => d.ToUniversalTime())
    {
    }
}
