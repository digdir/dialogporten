using System.Globalization;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Update;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Content;
using Digdir.Domain.Dialogporten.Domain.Http;
using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using Digdir.Domain.Dialogporten.WebApi.Common.Swagger;
using Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Common.Extensions;
using FastEndpoints;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.Dialogs.Update;

internal abstract class UpdateDialogSwaggerConfig : ISwaggerConfig
{
    public static string OperationId => "ReplaceDialog";

    public static RouteHandlerBuilder SetDescription(RouteHandlerBuilder builder) =>
        builder
            .OperationId(OperationId)
            .ProducesOneOf(
                StatusCodes.Status204NoContent,
                StatusCodes.Status400BadRequest,
                StatusCodes.Status404NotFound,
                StatusCodes.Status412PreconditionFailed,
                StatusCodes.Status422UnprocessableEntity);

    // TODO: Create fakers
    public static object GetExample() => new UpdateDialogDto
    {
        Progress = 42,
        ExtendedStatus = "Some extended status",
        ExternalReference = "Some external reference",
        Status = DialogStatus.Values.New,
        SearchTags =
        [
            new UpdateDialogSearchTagDto
            {
                Value = "searchTag"
            },
            new UpdateDialogSearchTagDto
            {
                Value = "anotherSearchTag"
            }
        ],
        Content =
        [
            new UpdateDialogContentDto
            {
                Type = DialogContentType.Values.Title,
                Value =
                [
                    new LocalizationDto
                    {
                        CultureCode = "en-us",
                        Value = "Some Title"
                    }
                ]
            },
            new UpdateDialogContentDto
            {
                Type = DialogContentType.Values.Summary,
                Value =
                [
                    new LocalizationDto
                    {
                        CultureCode = "en-us",
                        Value = "Some Summary"
                    }
                ]
            }
        ],
        VisibleFrom =
            DateTimeOffset.Parse("2054-03-04T12:13:10.0134400+00:00", CultureInfo.InvariantCulture),
        ExpiresAt = DateTimeOffset.Parse("2095-05-04T12:13:10.0134400+00:00", CultureInfo.InvariantCulture),
        DueAt = DateTimeOffset.Parse("2084-04-04T12:13:10.0134400+00:00", CultureInfo.InvariantCulture),
        Elements =
        [
            new UpdateDialogDialogElementDto
            {
                Id = Guid.Parse("02a72809-eddd-4192-864d-8f1755d72f4e"),
                Type = new Uri("http://example.com/some-type"),
                DisplayName =
                [
                    new LocalizationDto
                    {
                        CultureCode = "en-us",
                        Value = "Some display name"
                    }
                ],
                Urls =
                [
                    new UpdateDialogDialogElementUrlDto
                    {
                        Id = Guid.Parse("858177cb-8584-4d10-a086-3a5defa7a6c3"),
                        MimeType = "application/json",
                        Url = new Uri("http://example.com/some-url")
                    }
                ]
            }
        ],
        GuiActions =
        [
            new UpdateDialogDialogGuiActionDto
            {
                Id = Guid.Parse("8c64ecc8-7678-44b2-8788-0b5852dd8fa0"),
                Action = "submit",
                Priority = DialogGuiActionPriority.Values.Primary,
                Url = new Uri("https://example.com/some-url"),
                IsBackChannel = false,
                IsDeleteAction = false,
                Title =
                [
                    new LocalizationDto
                    {
                        CultureCode = "en-us",
                        Value = "GUI action title"
                    },
                    new LocalizationDto
                    {
                        CultureCode = "nb-no",
                        Value = "GUI action-tittel"
                    }
                ]
            }
        ],
        ApiActions =
        [
            new UpdateDialogDialogApiActionDto
            {
                Id = Guid.Parse("948b07ba-1a82-403e-8eaa-2e5784af07a9"),
                Action = "submit",
                Endpoints =
                [
                    new UpdateDialogDialogApiActionEndpointDto
                    {
                        Version = "20231015",
                        HttpMethod = HttpVerb.Values.POST,
                        Deprecated = false,
                        Url = new Uri("https://example.com/some-api-action"),
                        DocumentationUrl = new Uri("https://example.com/some-api-action-doc"),
                        RequestSchema = new Uri("https://example.com/some-api-action-request-schema"),
                        ResponseSchema = new Uri("https://example.com/some-api-action-response-schema")
                    }
                ]
            }
        ],
        Activities =
        [
            new UpdateDialogDialogActivityDto
            {
                Id = Guid.Parse("8b95d42d-d2b6-4c01-8ca0-a817a4b3c50d"),
                Type = DialogActivityType.Values.Information,
                PerformedBy =
                [
                    new LocalizationDto
                    {
                        CultureCode = "en-us",
                        Value = "Some performer"
                    },
                    new LocalizationDto
                    {
                        CultureCode = "nb-no",
                        Value = "En utfører"
                    }
                ],
                Description =
                [
                    new LocalizationDto
                    {
                        CultureCode = "en-us",
                        Value = "Some description"
                    },
                    new LocalizationDto
                    {
                        CultureCode = "nb-no",
                        Value = "En beskrivelse"
                    }
                ]
            }
        ]
    };
}

public sealed class UpdateDialogEndpointSummary : Summary<UpdateDialogEndpoint>
{
    public UpdateDialogEndpointSummary()
    {
        Summary = "Replaces a dialog";
        Description = $"""
                       Replaces a given dialog with the supplied model. For more information see the documentation (link TBD).

                       {Constants.SwaggerSummary.OptimisticConcurrencyNote}
                       """;
        Responses[StatusCodes.Status204NoContent] = Constants.SwaggerSummary.Updated.FormatInvariant("aggregate");
        Responses[StatusCodes.Status400BadRequest] = Constants.SwaggerSummary.ValidationError;
        Responses[StatusCodes.Status401Unauthorized] =
            Constants.SwaggerSummary.ServiceOwnerAuthenticationFailure.FormatInvariant(AuthorizationScope
                .ServiceProvider);
        Responses[StatusCodes.Status403Forbidden] =
            Constants.SwaggerSummary.AccessDeniedToDialog.FormatInvariant("update");
        Responses[StatusCodes.Status404NotFound] = Constants.SwaggerSummary.DialogNotFound;
        Responses[StatusCodes.Status412PreconditionFailed] = Constants.SwaggerSummary.RevisionMismatch;
        Responses[StatusCodes.Status422UnprocessableEntity] = Constants.SwaggerSummary.DomainError;
    }
}
