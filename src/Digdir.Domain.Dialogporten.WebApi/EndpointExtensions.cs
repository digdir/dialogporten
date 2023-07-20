using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using FastEndpoints;
using FluentValidation.Results;

namespace Digdir.Domain.Dialogporten.WebApi;

public static class EndpointExtensions
{
    public static Task BadRequestAsync(this IEndpoint ep, ValidationError failure, CancellationToken cancellationToken = default)
        => ep.BadRequestAsync(failure.Errors, cancellationToken);
    public static Task BadRequestAsync(this IEndpoint ep, IEnumerable<ValidationFailure> failures, CancellationToken cancellationToken = default)
        => ep.HttpContext.Response.SendErrorsAsync(failures.ToList() ?? new(), StatusCodes.Status400BadRequest, cancellation: cancellationToken);

    public static Task PreconditionFailed(this IEndpoint ep, CancellationToken cancellationToken = default)
        => ep.HttpContext.Response.SendErrorsAsync(new List<ValidationFailure>(), StatusCodes.Status412PreconditionFailed, cancellation: cancellationToken);

    public static Task NotFoundAsync(this IEndpoint ep, EntityNotFound notFound, CancellationToken cancellationToken = default) 
        => ep.HttpContext.Response.SendErrorsAsync(
            notFound.ToValidationResults(), 
            StatusCodes.Status404NotFound, 
            cancellation: cancellationToken);

    public static Task UnprocessableEntityAsync(this IEndpoint ep, DomainError domainError, CancellationToken cancellationToken = default)
        => ep.HttpContext.Response.SendErrorsAsync(
            domainError.ToValidationResults(),
            StatusCodes.Status422UnprocessableEntity,
            cancellation: cancellationToken);
}