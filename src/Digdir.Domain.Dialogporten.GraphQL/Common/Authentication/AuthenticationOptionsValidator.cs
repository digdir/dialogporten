using FluentValidation;

namespace Digdir.Domain.Dialogporten.GraphQL.Common.Authentication;

internal sealed class AuthenticationOptionsValidator : AbstractValidator<AuthenticationOptions>
{
    public AuthenticationOptionsValidator(
        IValidator<JwtBearerTokenSchemasOptions> jwtTokenSchemaValidator)
    {
        RuleFor(x => x.JwtBearerTokenSchemas)
            .NotEmpty()
            .WithMessage("At least one JwtBearerTokenSchema must be configured");
        RuleForEach(x => x.JwtBearerTokenSchemas)
            .SetValidator(jwtTokenSchemaValidator);
    }
}

internal sealed class JwtBearerTokenSchemasOptionsValidator : AbstractValidator<JwtBearerTokenSchemasOptions>
{
    public JwtBearerTokenSchemasOptionsValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.WellKnown).NotEmpty();
    }
}
