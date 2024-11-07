// using Altinn.Apiclient.Serviceowner.Interfaces;
// using Digdir.Library.Dialogporten.WebApiClient.Interfaces;
// using Refit;
//
// namespace Digdir.Library.Dialogporten.WebApiClient.Features.V1;
//
// public sealed class DialogportenClient(IDialogportenApi dialogportenApi) : IDialogportenClient
// {
//     public Task<IApiResponse<PaginatedListOfV1ServiceOwnerDialogsQueriesSearch_Dialog>> GetDialogList(
//         V1ServiceOwnerDialogsSearchSearchDialogQueryParams? param = null, CancellationToken cancellationToken = default) =>
//         dialogportenApi.V1ServiceOwnerDialogsSearchSearchDialog(param!, cancellationToken);
//
//     public Task<IApiResponse<V1ServiceOwnerDialogsQueriesGet_Dialog>> GetDialog(Guid dialogId, string? endUserId = null, CancellationToken cancellationToken = default) =>
//         dialogportenApi.V1ServiceOwnerDialogsGetGetDialog(dialogId, endUserId!, cancellationToken);
//
//     public Task<IApiResponse<string>> CreateDialog(V1ServiceOwnerDialogsCommandsCreate_DialogCommand createDialogCommand, CancellationToken cancellationToken = default) =>
//         dialogportenApi.V1ServiceOwnerDialogsCreateDialog(createDialogCommand, cancellationToken);
//
//     public Task<IApiResponse> DeleteDialog(Guid dialogId, Guid? ifMatch = null, CancellationToken cancellationToken = default) =>
//         dialogportenApi.V1ServiceOwnerDialogsDeleteDialog(dialogId, ifMatch, cancellationToken);
//
//     public Task<IApiResponse> PurgeDialog(Guid dialogId, Guid? ifMatch = null, CancellationToken cancellationToken = default) =>
//         dialogportenApi.V1ServiceOwnerDialogsPurgePurgeDialog(dialogId, ifMatch, cancellationToken);
//
//     public Task<IApiResponse> PatchDialog(Guid dialogId, IEnumerable<JsonPatchOperations_Operation> PatchDocument, Guid? ifMatch = null, CancellationToken cancellationToken = default) =>
//         dialogportenApi.V1ServiceOwnerDialogsPatchDialog(dialogId, PatchDocument, ifMatch, cancellationToken);
//
//     public Task<IApiResponse> UpdateDialog(Guid dialogId, V1ServiceOwnerDialogsCommandsUpdate_Dialog updateCommand, Guid? ifMatch, CancellationToken cancellationToken = default) =>
//         dialogportenApi.V1ServiceOwnerDialogsUpdateDialog(dialogId, updateCommand, ifMatch, cancellationToken);
// }
