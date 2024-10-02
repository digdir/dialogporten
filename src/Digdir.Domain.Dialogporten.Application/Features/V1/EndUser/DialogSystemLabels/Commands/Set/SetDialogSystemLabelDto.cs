using System.Diagnostics.CodeAnalysis;
using Digdir.Domain.Dialogporten.Domain.DialogEndUserContexts.Entities;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogSystemLabels.Commands.Set;

public class SetDialogSystemLabelDto
{
    public Guid DialogId { get; set; }
    public SystemLabel.Values Label { get; set; }
}

// Amund: Trengs denne lenger?
public sealed class SetDialogSystemLabelLabelDto : IParsable<SetDialogSystemLabelLabelDto>
{
    public string Label { get; }
    public string Namespace { get; }
    public string FullName { get; }
    public SystemLabel.Values? SystemLabel { get; init; }

    public SetDialogSystemLabelLabelDto(string ns, string label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(label));
        }

        if (string.IsNullOrWhiteSpace(ns))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(ns));
        }

        Label = label;
        Namespace = ns;
        FullName = $"{ns}:{label}";
    }

    [SuppressMessage("Style", "IDE0055:Fix formatting")]
    public static bool TryParse(string? s, IFormatProvider? provider, [NotNullWhen(true)] out SetDialogSystemLabelLabelDto? result)
    {
        if (s is null)
        {
            result = null;
            return false;
        }

        result = s.Split(':') switch
        {
            [var @namespace, var systemLabel] when
                string.Equals(@namespace, Domain.DialogEndUserContexts.Entities.SystemLabel.Prefix, StringComparison.OrdinalIgnoreCase)
                && Enum.TryParse<SystemLabel.Values>(systemLabel, ignoreCase: true, out var actualLabel) =>
                new SetDialogSystemLabelLabelDto(@namespace, systemLabel)
                {
                    SystemLabel = actualLabel
                },
            // [var @namespace, var labelString] when 
            //     string.Equals(@namespace, "label", StringComparison.OrdinalIgnoreCase) => 
            //     new SetDialogLabelLabelDto(@namespace, labelString),
            _ => null
        };

        return result is not null;
    }

    public static SetDialogSystemLabelLabelDto Parse(string s, IFormatProvider? provider) =>
        !TryParse(s, provider, out var result)
            ? throw new ArgumentException("Could not parse supplied value.", nameof(s))
            : result;
}
