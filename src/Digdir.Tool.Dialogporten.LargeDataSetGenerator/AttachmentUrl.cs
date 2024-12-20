using System.Text;
using Medo;

namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;
#pragma warning disable CA1305

internal static class AttachmentUrl
{
    public const string CopyCommand = "COPY \"AttachmentUrl\" (\"Id\", \"CreatedAt\", \"MediaType\", \"Url\", \"ConsumerTypeId\", \"AttachmentId\") FROM STDIN (FORMAT csv, HEADER false, NULL '')";

    public static string Generate(DateTimeOffset currentDate, DateTimeOffset endDate, TimeSpan intervalSeconds)
    {
        var attachmentUrlCsvData = new StringBuilder();

        while (currentDate < endDate)
        {
            var attachmentId = DeterministicUuidV7.Generate(currentDate, nameof(Domain.Dialogporten.Domain.Attachments.Attachment));

            var attachmentUrlId1 = DeterministicUuidV7.Generate(currentDate, nameof(Domain.Dialogporten.Domain.Attachments.AttachmentUrl), 1);
            var attachmentUrlId2 = DeterministicUuidV7.Generate(currentDate, nameof(Domain.Dialogporten.Domain.Attachments.AttachmentUrl), 2);

            var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
            attachmentUrlCsvData.AppendLine($"{attachmentUrlId1},{formattedDate},'text/plain','https://digdir.apps.tt02.altinn.no/',1,{attachmentId}");
            attachmentUrlCsvData.AppendLine($"{attachmentUrlId2},{formattedDate},'text/plain','https://digdir.apps.tt02.altinn.no/',2,{attachmentId}");

            currentDate = currentDate.Add(intervalSeconds);
        }

        return attachmentUrlCsvData.ToString();
    }
}
