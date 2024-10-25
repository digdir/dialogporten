namespace Digdir.Domain.Dialogporten.WebApi.Common.Swagger;

public interface ISwaggerConfig
{

    static abstract RouteHandlerBuilder SetDescription(RouteHandlerBuilder builder, Type type);
    // TODO: Does this need to be split in two? One for request, one for response
    static abstract object GetExample();
}
