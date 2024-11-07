// using Altinn.Apiclient.Serviceowner.Interfaces;
// using Refit;
//
// namespace Digdir.Library.Dialogporten.WebApiClient.Interfaces;
//
// public interface IDialogportenClient
// {
//     // Amund: Dette er et "lag" over refitter, har fjernes kindof versjon fra metodekallet og blir styrt av parameter.
//     // Men om den er bare der for å rydde i metodenavn så er den unødvendig siden vi ikke rydder i typenavn?
//     // Hvorfor ikke bare brukte refit direkte? Da er denne "SDKen" ekstremt liten. og egt bare en extension til ServiceCollection.
//     // Det blir da ikke mye vedlikehold i forhold til at den har men funksjonalitet og abstraksjon
//     // men er det da vits å ha refit her? og heller bare lage en dialogporten extension?
//     // Amund: føler at jeg mister hvorfor denne skal lages, og hvilket behov den egt skal dekke. Im losing the plot.
//     Task<IApiResponse<PaginatedListOfV1ServiceOwnerDialogsQueriesSearch_Dialog>> GetDialogList(
//         V1ServiceOwnerDialogsSearchSearchDialogQueryParams? param = null,
//         CancellationToken cancellationToken = default);
//
//     Task<IApiResponse<V1ServiceOwnerDialogsQueriesGet_Dialog>> GetDialog(
//         Guid dialogId,
//         string? endUserId = null,
//         CancellationToken cancellationToken = default);
//
//     Task<IApiResponse<string>> CreateDialog(
//         V1ServiceOwnerDialogsCommandsCreate_DialogCommand createDialogCommand,
//         CancellationToken cancellationToken = default);
//
//     Task<IApiResponse> DeleteDialog(
//         Guid dialogId,
//         Guid? ifMatch,
//         CancellationToken cancellationToken = default);
//
//     Task<IApiResponse> PurgeDialog(
//         Guid dialogId,
//         Guid? ifMatch,
//         CancellationToken cancellationToken = default);
//
//     Task<IApiResponse> PatchDialog(
//         Guid dialogId,
//         IEnumerable<JsonPatchOperations_Operation> PatchDocument,
//         Guid? ifMatch,
//         CancellationToken cancellationToken = default);
//
//     Task<IApiResponse> UpdateDialog(
//         Guid dialogId,
//         V1ServiceOwnerDialogsCommandsUpdate_Dialog updateCommand,
//         Guid? ifMatch,
//         CancellationToken cancellationToken = default);
// }
