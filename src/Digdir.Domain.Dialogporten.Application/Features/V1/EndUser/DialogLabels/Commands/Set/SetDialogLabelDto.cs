using System.Diagnostics.CodeAnalysis;
using Digdir.Domain.Dialogporten.Domain.DialogEndUserContexts.Entities;
using SystemLabelEntity = Digdir.Domain.Dialogporten.Domain.DialogEndUserContexts.Entities.SystemLabel;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogLabels.Commands.Set;

public class SetDialogLabelDto
{
    public Guid DialogId { get; set; }
    public string Label { get; set; } = null!;
}

public sealed class SetDialogLabelLabelDto : IParsable<SetDialogLabelLabelDto>
{
    public string Label { get; }
    public string Namespace { get; }
    public string FullName { get; }
    public SystemLabel.Values? SystemLabel { get; init; }

    public SetDialogLabelLabelDto(string ns, string label)
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
    public static bool TryParse(string? s, IFormatProvider? provider, [NotNullWhen(true)] out SetDialogLabelLabelDto? result)
    {
        if (s is null)
        {
            result = null;
            return false;
        }

        result = s.Split(':') switch
        {
            [var @namespace, var systemLabel] when 
                string.Equals(@namespace, SystemLabelEntity.Prefix, StringComparison.OrdinalIgnoreCase)
                && Enum.TryParse<SystemLabelEntity.Values>(systemLabel, ignoreCase: true, out var actualLabel) =>
                new SetDialogLabelLabelDto(@namespace, systemLabel)
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

    public static SetDialogLabelLabelDto Parse(string s, IFormatProvider? provider) =>
        !TryParse(s, provider, out var result)
            ? throw new ArgumentException("Could not parse supplied value.", nameof(s))
            : result;
}
