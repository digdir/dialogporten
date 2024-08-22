using Digdir.Domain.Dialogporten.Application.Common.Extensions.FluentValidation;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Domain;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Contents;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions.Contents;
using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.Content;

// DialogContentValueDtoValidator has constructor parameter input, and can't be registered in DI assembly scan
// This interface is used to ignore the class when scanning for validators
// The validator is manually created in the Create and Update validators
internal interface IIgnoreOnAssemblyScan;

internal sealed class ContentValueDtoValidator : AbstractValidator<ContentValueDto>, IIgnoreOnAssemblyScan
{
    public ContentValueDtoValidator(DialogTransmissionContentType contentType)
    {
        RuleFor(x => x.MediaType)
            .NotEmpty()
            .Must(value => value is not null && contentType.AllowedMediaTypes.Contains(value))
            .WithMessage($"{{PropertyName}} '{{PropertyValue}}' is not allowed for content type {contentType.Name}. " +
                         $"Allowed media types are {string.Join(", ", contentType.AllowedMediaTypes.Select(x => $"'{x}'"))}");
        RuleForEach(x => x.Value)
            .ContainsValidHtml()
            .When(x => x.MediaType is not null and MediaTypes.Html);
        RuleForEach(x => x.Value)
            .ContainsValidMarkdown()
            .When(x => x.MediaType is not null and MediaTypes.Markdown);
        RuleForEach(x => x.Value)
            .Must(x => Uri.TryCreate(x.Value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps)
            .When(x => x.MediaType is not null && x.MediaType.StartsWith(MediaTypes.EmbeddablePrefix, StringComparison.InvariantCultureIgnoreCase))
            .WithMessage("{PropertyName} must be a valid HTTPS URL for embeddable content types");
        RuleFor(x => x.Value)
            .NotEmpty()
            .SetValidator(_ => new LocalizationDtosValidator(contentType.MaxLength));
    }

    public ContentValueDtoValidator(DialogContentType contentType)
    {
        RuleFor(x => x.MediaType)
            .NotEmpty()
            .Must(value => value is not null && contentType.AllowedMediaTypes.Contains(value))
            .WithMessage($"{{PropertyName}} '{{PropertyValue}}' is not allowed for content type {contentType.Name}. " +
                         $"Allowed media types are {string.Join(", ", contentType.AllowedMediaTypes.Select(x => $"'{x}'"))}");
        RuleForEach(x => x.Value)
            .ContainsValidHtml()
            .When(x => x.MediaType is not null and MediaTypes.Html);
        RuleForEach(x => x.Value)
            .ContainsValidMarkdown()
            .When(x => x.MediaType is not null and MediaTypes.Markdown);
        RuleForEach(x => x.Value)
            .Must(x => Uri.TryCreate(x.Value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps)
            .When(x => x.MediaType is not null && x.MediaType.StartsWith(MediaTypes.EmbeddablePrefix, StringComparison.InvariantCultureIgnoreCase))
            .WithMessage("{PropertyName} must be a valid HTTPS URL for embeddable content types");
        RuleFor(x => x.Value)
            .NotEmpty()
            .SetValidator(_ => new LocalizationDtosValidator(contentType.MaxLength));
    }
}
