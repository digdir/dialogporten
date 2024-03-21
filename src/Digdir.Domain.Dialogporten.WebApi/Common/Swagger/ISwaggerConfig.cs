namespace Digdir.Domain.Dialogporten.WebApi.Common.Swagger;

public interface ISwaggerConfig
{
    public static abstract string OperationId { get; }
    static abstract RouteHandlerBuilder SetDescription(RouteHandlerBuilder builder);
    // TODO: Does this need to be split in two? One for request, one for response
    static abstract object GetExample();
}
