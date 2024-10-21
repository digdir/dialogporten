namespace Digdir.Domain.Dialogporten.Infrastructure;

public static class HttpResponseMessageExtensions
{
    private const int MaxContentLength = 1000;

    public static async Task<HttpResponseMessage> EnsureSuccessStatusCodeWithContent(this HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return response;
        }

        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(content))
        {
            // This will throw HttpRequestException with the response status code and reason phrase
            return response.EnsureSuccessStatusCode();
        }

        if (content.Length > MaxContentLength)
        {
            content = content[..MaxContentLength] + "[truncated after " + MaxContentLength + " characters]";
        }

        throw new HttpRequestException("Response unsuccessful (" + response.GetResponseCodeWithReasonPhrase() +
                                       " with error content: " + content, null, response.StatusCode);

    }

    private static string GetResponseCodeWithReasonPhrase(this HttpResponseMessage response) =>
        string.IsNullOrWhiteSpace(response.ReasonPhrase)
            ? response.StatusCode.ToString()
            : response.StatusCode + " " + response.ReasonPhrase;
}
