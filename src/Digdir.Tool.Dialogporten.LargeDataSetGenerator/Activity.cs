// using System.Text;
// using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
// using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
//
// namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;
// #pragma warning disable CA1305
//
// internal static class Activity
// {
//     public const string CopyCommand = """COPY "DialogActivity" ("Id", "CreatedAt", "ExtendedType", "TypeId", "TransmissionId", "DialogId") FROM STDIN (FORMAT csv, HEADER false, NULL '')""";
//
//     public const int DialogCreatedType = 1;
//     public const int InformationType = 2;
//
//     public const int EntitiesPerParent = 2;
//     public const int EntitiesPerBatch = EntitiesPerParent * Dialog.EntitiesPerBatch;
//
//     public static string Generate(DateTimeOffset currentDate, DateTimeOffset endDate, TimeSpan interval)
//     {
//         var activityCsvData = new StringBuilder();
//
//         var dialogIds = Enumerable.Range(0, EntitiesPerBatch)
//             .Select(x => currentDate + interval * x)
//             .Select(x => (Timestamp: x, DialogId: DeterministicUuidV7.Generate(x, nameof(DialogEntity))));
//
//         foreach (var (timestamp, dialogId) in dialogIds)
//         {
//             var activityId1 = DeterministicUuidV7.Generate(timestamp, nameof(DialogActivity), DialogCreatedType);
//             var activityId2 = DeterministicUuidV7.Generate(timestamp, nameof(DialogActivity), InformationType);
//
//             var formattedDate = timestamp.ToString("yyyy-MM-dd HH:mm:ss zzz");
//             activityCsvData.AppendLine($"{activityId1},{formattedDate},NULL,1,,{dialogId}"); // Type DialogCreated
//             activityCsvData.AppendLine($"{activityId2},{formattedDate},NULL,3,,{dialogId}"); // Type Information
//
//         }
//
//         // while (currentDate < endDate) {
//         //
//         //     var dialogId = DeterministicUuidV7.Generate(currentDate, nameof(DialogEntity));
//         //
//         //     var activityId1 = DeterministicUuidV7.Generate(currentDate, nameof(DialogActivity), DialogCreatedType);
//         //     var activityId2 = DeterministicUuidV7.Generate(currentDate, nameof(DialogActivity), InformationType);
//         //
//         //
//         //     var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
//         //     activityCsvData.AppendLine($"{activityId1},{formattedDate},NULL,1,,{dialogId}"); // Type DialogCreated
//         //     activityCsvData.AppendLine($"{activityId2},{formattedDate},NULL,3,,{dialogId}"); // Type Information
//         //
//         //     currentDate = currentDate.Add(interval);
//         // }
//
//         return activityCsvData.ToString();
//     }
// }
