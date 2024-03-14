using Microsoft.Extensions.Options;

namespace Digdir.Domain.Dialogporten.Application.Unit.Tests.Common;

internal sealed class OptionsMock<T> : IOptions<T> where T : class
{
    public T Value { get; set; }

    public OptionsMock(T value)
    {
        Value = value;
    }
}
