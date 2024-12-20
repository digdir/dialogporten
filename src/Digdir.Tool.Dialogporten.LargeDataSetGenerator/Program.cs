using System.Diagnostics;
using System.Globalization;
using Digdir.Tool.Dialogporten.LargeDataSetGenerator;
using Npgsql;
using Activity = Digdir.Tool.Dialogporten.LargeDataSetGenerator.Activity;

#pragma warning disable CA1305

try
{
    Console.WriteLine("Starting large data set generator...");

    var connString = Environment.GetEnvironmentVariable("CONN_STRING");
    var intervalSeconds = int.Parse(Environment.GetEnvironmentVariable("INTERVAL_SECONDS")!);
    var batchNumDays = int.Parse(Environment.GetEnvironmentVariable("BATCH_NUM_DAYS")!);
    var startingYear = int.Parse(Environment.GetEnvironmentVariable("STARTING_YEAR")!);
    var totalNumMonths = int.Parse(Environment.GetEnvironmentVariable("TOTAL_NUM_MONTHS")!);

    var startDateString = $"{startingYear}/01/01 00:00:00 +00:00";
    var currentDate = DateTimeOffset.ParseExact(
        startDateString, "yyyy/MM/dd HH:mm:ss zzz",
        CultureInfo.InvariantCulture);

    var endDate = currentDate.AddMonths(totalNumMonths);

    var totalDialogCreatedStartTimestamp = Stopwatch.GetTimestamp();

    var dialogsCreated = 0;
    var tasks = new List<Task>();

    while (currentDate < endDate)
    {
        var batchEndDate = currentDate.AddDays(batchNumDays);
        Console.WriteLine($"Generating dialogs from {currentDate} to {batchEndDate}...");

        var (dialogIds, dialogCsvData) = Dialog.Generate(currentDate, batchEndDate, intervalSeconds);

        tasks.Add(Task.Run(async () =>
        {
            var dialogStartTimestamp = Stopwatch.GetTimestamp();
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();
            await using var writer = conn.BeginTextImport(Dialog.CopyCommand);
            await writer.WriteAsync(dialogCsvData);
            Console.WriteLine($"Inserted {dialogIds.Count} dialogs... it took {Stopwatch.GetElapsedTime(dialogStartTimestamp)}");
        }));


        // Dialog Content:
        var (dialogContentIds, dialogContentCsvData) = DialogContent.Generate(dialogIds, currentDate, intervalSeconds);

        tasks.Add(Task.Run(async () =>
        {
            var dialogContentStartTimestamp = Stopwatch.GetTimestamp();
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();
            await using var contentWriter = conn.BeginTextImport(DialogContent.CopyCommand);
            await contentWriter.WriteAsync(dialogContentCsvData);
            Console.WriteLine($"Inserted {dialogContentIds.Count} dialog content for {dialogIds.Count} dialogs... it took {Stopwatch.GetElapsedTime(dialogContentStartTimestamp)}");
        }));


        // Dialog Gui Action:
        var (guiActionIds, dialogGuiActionCsvData) = GuiAction.Generate(dialogIds, currentDate, intervalSeconds);

        tasks.Add(Task.Run(async () =>
        {
            var dialogGuiActionStartTimestamp = Stopwatch.GetTimestamp();
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();
            await using var guiActionWriter = conn.BeginTextImport(GuiAction.CopyCommand);
            await guiActionWriter.WriteAsync(dialogGuiActionCsvData);
            Console.WriteLine($"Inserted {guiActionIds.Count} dialog gui actions for {dialogIds.Count} dialogs... it took {Stopwatch.GetElapsedTime(dialogGuiActionStartTimestamp)}");
        }));


        // EndUserContext:
        var endUserContextCsvData = EndUserContext.Generate(dialogIds, currentDate, intervalSeconds);

        tasks.Add(Task.Run(async () =>
        {
            var endUserContextStartTimestamp = Stopwatch.GetTimestamp();
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();
            await using var endUserContextWriter = conn.BeginTextImport(EndUserContext.CopyCommand);
            await endUserContextWriter.WriteAsync(endUserContextCsvData);
            Console.WriteLine($"Inserted {dialogIds.Count} end user contexts for {dialogIds.Count} dialogs... it took {Stopwatch.GetElapsedTime(endUserContextStartTimestamp)}");
        }));


        // SeenLog:
        var (seenLogIds, seenLogCsvData) = SeenLog.Generate(dialogIds, currentDate, intervalSeconds);

        tasks.Add(Task.Run(async () =>
        {
            var seenLogStartTimestamp = Stopwatch.GetTimestamp();
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();
            await using var seenLogWriter = conn.BeginTextImport(SeenLog.CopyCommand);
            await seenLogWriter.WriteAsync(seenLogCsvData);
            Console.WriteLine($"Inserted {seenLogIds.Count} seen logs for {dialogIds.Count} dialogs... it took {Stopwatch.GetElapsedTime(seenLogStartTimestamp)}");
        }));


        // SearchTags:
        var searchTagCsvData = SearchTags.Generate(dialogIds, currentDate, intervalSeconds);

        tasks.Add(Task.Run(async () =>
        {
            var searchTagStartTimestamp = Stopwatch.GetTimestamp();
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();
            await using var searchTagWriter = conn.BeginTextImport(SearchTags.CopyCommand);
            await searchTagWriter.WriteAsync(searchTagCsvData);
            Console.WriteLine($"Inserted search tags for {dialogIds.Count} dialogs... it took {Stopwatch.GetElapsedTime(searchTagStartTimestamp)}");
        }));


        // Transmission:
        var (transmissionIds, transmissionCsvData) = Transmission.Generate(dialogIds, currentDate, intervalSeconds);

        tasks.Add(Task.Run(async () =>
        {
            var transmissionStartTimestamp = Stopwatch.GetTimestamp();
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();
            await using var transmissionWriter = conn.BeginTextImport(Transmission.CopyCommand);
            await transmissionWriter.WriteAsync(transmissionCsvData);
            Console.WriteLine($"Inserted {transmissionIds.Count} transmissions for {dialogIds.Count} dialogs... it took {Stopwatch.GetElapsedTime(transmissionStartTimestamp)}");
        }));


        // TransmissionContent:
        var (transmissionContentIds, transmissionContentCsvData) = TransmissionContent.Generate(transmissionIds, currentDate, intervalSeconds);

        tasks.Add(Task.Run(async () =>
        {
            var transmissionContentStartTimestamp = Stopwatch.GetTimestamp();
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();
            await using var transmissionContentWriter = conn.BeginTextImport(TransmissionContent.CopyCommand);
            await transmissionContentWriter.WriteAsync(transmissionContentCsvData);
            Console.WriteLine(
                $"Inserted {transmissionContentIds.Count} transmission content for {transmissionIds.Count} transmissions... it took {Stopwatch.GetElapsedTime(transmissionContentStartTimestamp)}");
        }));


        // Activity:
        var (activityIds, activityCsvData) = Activity.Generate(dialogIds, currentDate, intervalSeconds);

        tasks.Add(Task.Run(async () =>
        {
            var activityStartTimestamp = Stopwatch.GetTimestamp();
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();
            await using var activityWriter = conn.BeginTextImport(Activity.CopyCommand);
            await activityWriter.WriteAsync(activityCsvData);
            Console.WriteLine($"Inserted {activityIds.Count} activities for {dialogIds.Count} dialogs... it took {Stopwatch.GetElapsedTime(activityStartTimestamp)}");
        }));


        // Attachment:
        var (attachmentIds, attachmentCsvData) = Attachment.Generate(dialogIds, transmissionIds, currentDate, intervalSeconds);

        tasks.Add(Task.Run(async () =>
        {
            var attachmentStartTimestamp = Stopwatch.GetTimestamp();
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();
            await using var attachmentWriter = conn.BeginTextImport(Attachment.CopyCommand);
            await attachmentWriter.WriteAsync(attachmentCsvData);
            Console.WriteLine($"Inserted {attachmentIds.Count} attachments for {dialogIds.Count} dialogs and {transmissionIds.Count} transmissions... it took {Stopwatch.GetElapsedTime(attachmentStartTimestamp)}");
        }));


        // AttachmentUrl:
        var attachmentUrlCsvData = AttachmentUrl.Generate(attachmentIds, currentDate, intervalSeconds);

        tasks.Add(Task.Run(async () =>
        {
            var attachmentUrlStartTimestamp = Stopwatch.GetTimestamp();
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();
            await using var attachmentUrlWriter = conn.BeginTextImport(AttachmentUrl.CopyCommand);
            await attachmentUrlWriter.WriteAsync(attachmentUrlCsvData);
            Console.WriteLine($"Inserted attachment urls for {attachmentIds.Count} attachments... it took {Stopwatch.GetElapsedTime(attachmentUrlStartTimestamp)}");
        }));



        // Actor:
        var actorCsvData = Actor.Generate(activityIds, seenLogIds, transmissionIds, currentDate, intervalSeconds);

        tasks.Add(Task.Run(async () =>
        {
            var actorStartTimestamp = Stopwatch.GetTimestamp();
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();
            await using var actorWriter = conn.BeginTextImport(Actor.CopyCommand);
            await actorWriter.WriteAsync(actorCsvData);
            Console.WriteLine($"Inserted actors for {dialogIds.Count} dialogs... it took {Stopwatch.GetElapsedTime(actorStartTimestamp)}");
        }));


        // LocalizationSet:
        var (localizationSetIds, localizationSetCsvData) = LocalizationSet.Generate(attachmentIds, guiActionIds, activityIds, dialogContentIds, transmissionContentIds, currentDate, intervalSeconds);

        tasks.Add(Task.Run(async () =>
        {
            var localizationSetStartTimestamp = Stopwatch.GetTimestamp();
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();
            await using var localizationSetWriter = conn.BeginTextImport(LocalizationSet.CopyCommand);
            await localizationSetWriter.WriteAsync(localizationSetCsvData);
            Console.WriteLine($"Inserted localization sets for {dialogIds.Count} dialogs... it took {Stopwatch.GetElapsedTime(localizationSetStartTimestamp)}");
        }));


        // Localization:
        var localizationCsvData = Localization.Generate(localizationSetIds, currentDate);

        tasks.Add(Task.Run(async () =>
        {
            var localizationStartTimestamp = Stopwatch.GetTimestamp();
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();
            await using var localizationWriter = conn.BeginTextImport(Localization.CopyCommand);
            await localizationWriter.WriteAsync(localizationCsvData);
            Console.WriteLine($"Inserted localizations for {localizationSetIds.Count} localization sets... it took {Stopwatch.GetElapsedTime(localizationStartTimestamp)}");
        }));

        await Task.WhenAll(tasks);

        dialogsCreated += dialogIds.Count;
        currentDate = batchEndDate;
        Console.WriteLine(string.Empty);
    }

    var timeItTook = Stopwatch.GetElapsedTime(totalDialogCreatedStartTimestamp);
    Console.WriteLine($"Generated {dialogsCreated} dialogs in {timeItTook}");
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
}

// TODO: Re-enable all indexes and constraints
#pragma warning restore CA1305
