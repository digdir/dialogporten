using Digdir.Library.Dialogporten.WebApiClient.Features.V1;
using Refit;

namespace Digdir.Library.Dialogporten.WebApiClient.Sample;

public sealed class Dialogs(IServiceownerApi client)
{
    public async Task<IApiResponse> Purge(Guid dialogId, Guid? ifMatch = null)
    {
        var response = await client.V1ServiceOwnerDialogsPurgePurgeDialog(dialogId, ifMatch);
        Console.WriteLine($"Purge response status code: {response.StatusCode}");
        Console.WriteLine($"Purge Response: {response.StatusCode}");
        return response;
    }

    public static void PrintGetDialog(V1ServiceOwnerDialogsQueriesGet_Dialog dialog)
    {
        Console.WriteLine($"System Label: {dialog.SystemLabel}");
        Console.WriteLine($"Dialog Status: {dialog.Status}");
        Console.WriteLine($"Dialog Org: {dialog.Org}");
        Console.WriteLine($"Dialog Progress: {dialog.Progress}");
        Console.WriteLine($"Deleted at: {dialog.DeletedAt}");
    }
}
