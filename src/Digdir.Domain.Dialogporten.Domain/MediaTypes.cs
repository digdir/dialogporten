using System.Net.Mime;
using System.Reflection;

namespace Digdir.Domain.Dialogporten.Domain;

public static class MediaTypes
{
    // Custom MediaTypes for embedding content in frontend applications
    private static readonly string[] CustomDialogportenMediaTypes =
        [
            "application/vnd.dialogporten.frontchannelembed+json;type=markdown"
        ];

    private static string[]? _validMediaTypes;

    public static bool IsValid(string mediaType) => GetValidMediaTypes().Contains(mediaType);

    private static string[] GetValidMediaTypes()
    {
        if (_validMediaTypes != null)
        {
            return _validMediaTypes;
        }

        var applicationMediaTypes = GetMediaTypes(typeof(MediaTypeNames.Application));
        var imageMediaTypes = GetMediaTypes(typeof(MediaTypeNames.Image));
        var textMediaTypes = GetMediaTypes(typeof(MediaTypeNames.Text));

        _validMediaTypes = applicationMediaTypes
            .Concat(imageMediaTypes)
            .Concat(textMediaTypes)
            .Concat(CustomDialogportenMediaTypes)
            .ToArray();

        return _validMediaTypes;
    }

    private static IEnumerable<string> GetMediaTypes(Type mediaTypeClass)
    {
        return mediaTypeClass
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.FieldType == typeof(string))
            .Select(field => (string)field.GetValue(null)!);
    }
}
