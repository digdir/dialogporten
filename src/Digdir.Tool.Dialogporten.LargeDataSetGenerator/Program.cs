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

    await using var dataSource = NpgsqlDataSource.Create(connString!);

    await using var dialogConnection = await dataSource.OpenConnectionAsync();
    await using var dialogWriter = dialogConnection.BeginTextImport(Dialog.CopyCommand);

    await using var dialogContentConnection = await dataSource.OpenConnectionAsync();
    await using var dialogContentWriter = dialogContentConnection.BeginTextImport(DialogContent.CopyCommand);

    await using var guiActionConnection = await dataSource.OpenConnectionAsync();
    await using var guiActionWriter = guiActionConnection.BeginTextImport(GuiAction.CopyCommand);

    await using var endUserContextConnection = await dataSource.OpenConnectionAsync();
    await using var endUserContextWriter = endUserContextConnection.BeginTextImport(EndUserContext.CopyCommand);

    await using var seenLogConnection = await dataSource.OpenConnectionAsync();
    await using var seenLogWriter = seenLogConnection.BeginTextImport(SeenLog.CopyCommand);

    await using var searchTagConnection = await dataSource.OpenConnectionAsync();
    await using var searchTagWriter = searchTagConnection.BeginTextImport(SearchTags.CopyCommand);

    await using var transmissionConnection = await dataSource.OpenConnectionAsync();
    await using var transmissionWriter = transmissionConnection.BeginTextImport(Transmission.CopyCommand);

    await using var transmissionContentConnection = await dataSource.OpenConnectionAsync();
    await using var transmissionContentWriter =
        transmissionContentConnection.BeginTextImport(TransmissionContent.CopyCommand);

    await using var activityConnection = await dataSource.OpenConnectionAsync();
    await using var activityWriter = activityConnection.BeginTextImport(Activity.CopyCommand);

    await using var attachmentConnection = await dataSource.OpenConnectionAsync();
    await using var attachmentWriter = attachmentConnection.BeginTextImport(Attachment.CopyCommand);

    await using var attachmentUrlConnection = await dataSource.OpenConnectionAsync();
    await using var attachmentUrlWriter = attachmentUrlConnection.BeginTextImport(AttachmentUrl.CopyCommand);

    await using var actorConnection = await dataSource.OpenConnectionAsync();
    await using var actorWriter = actorConnection.BeginTextImport(Actor.CopyCommand);

    await using var localizationSetConnection = await dataSource.OpenConnectionAsync();
    await using var localizationSetWriter = localizationSetConnection.BeginTextImport(LocalizationSet.CopyCommand);

    await using var localizationConnection = await dataSource.OpenConnectionAsync();
    await using var localizationWriter = localizationConnection.BeginTextImport(Localization.CopyCommand);


    while (currentDate < endDate)
    {
        var batchEndDate = currentDate.AddDays(batchNumDays);
        Console.WriteLine($"Generating dialogs from {currentDate} to {batchEndDate}...");

        var (dialogIds, dialogCsvData) = Dialog.Generate(currentDate, batchEndDate, intervalSeconds);

        tasks.Add(Task.Run(async () =>
        {
            var dialogStartTimestamp = Stopwatch.GetTimestamp();
            // await using var conn = new NpgsqlConnection(connString);
            // await using var conn = await dataSource.OpenConnectionAsync();
            // await using var writer = conn.BeginTextImport(Dialog.CopyCommand);
            await dialogWriter.WriteAsync(dialogCsvData);
            Console.WriteLine(
                $"Inserted {dialogIds.Count} dialogs... it took {Stopwatch.GetElapsedTime(dialogStartTimestamp)}");
        }));


        // Dialog Content:
        var (dialogContentIds, dialogContentCsvData) = DialogContent.Generate(dialogIds, currentDate, intervalSeconds);

        tasks.Add(Task.Run(async () =>
        {
            var dialogContentStartTimestamp = Stopwatch.GetTimestamp();
            // await using var conn = await dataSource.OpenConnectionAsync();

            // await using var contentWriter = conn.BeginTextImport(DialogContent.CopyCommand);
            await dialogContentWriter.WriteAsync(dialogContentCsvData);
            Console.WriteLine(
                $"Inserted {dialogContentIds.Count} dialog content for {dialogIds.Count} dialogs... it took {Stopwatch.GetElapsedTime(dialogContentStartTimestamp)}");
        }));


        // Dialog Gui Action:
        var (guiActionIds, dialogGuiActionCsvData) = GuiAction.Generate(dialogIds, currentDate, intervalSeconds);

        tasks.Add(Task.Run(async () =>
        {
            var dialogGuiActionStartTimestamp = Stopwatch.GetTimestamp();

            await guiActionWriter.WriteAsync(dialogGuiActionCsvData);
            Console.WriteLine(
                $"Inserted {guiActionIds.Count} dialog gui actions for {dialogIds.Count} dialogs... it took {Stopwatch.GetElapsedTime(dialogGuiActionStartTimestamp)}");
        }));


        // EndUserContext:
        var endUserContextCsvData = EndUserContext.Generate(dialogIds, currentDate, intervalSeconds);

        tasks.Add(Task.Run(async () =>
        {
            var endUserContextStartTimestamp = Stopwatch.GetTimestamp();
            await endUserContextWriter.WriteAsync(endUserContextCsvData);
            Console.WriteLine(
                $"Inserted {dialogIds.Count} end user contexts for {dialogIds.Count} dialogs... it took {Stopwatch.GetElapsedTime(endUserContextStartTimestamp)}");
        }));


        // SeenLog:
        var (seenLogIds, seenLogCsvData) = SeenLog.Generate(dialogIds, currentDate, intervalSeconds);

        tasks.Add(Task.Run(async () =>
        {
            var seenLogStartTimestamp = Stopwatch.GetTimestamp();
            await seenLogWriter.WriteAsync(seenLogCsvData);
            Console.WriteLine(
                $"Inserted {seenLogIds.Count} seen logs for {dialogIds.Count} dialogs... it took {Stopwatch.GetElapsedTime(seenLogStartTimestamp)}");
        }));


        // SearchTags:
        var searchTagCsvData = SearchTags.Generate(dialogIds, currentDate, intervalSeconds);

        tasks.Add(Task.Run(async () =>
        {
            var searchTagStartTimestamp = Stopwatch.GetTimestamp();
            await searchTagWriter.WriteAsync(searchTagCsvData);
            Console.WriteLine(
                $"Inserted search tags for {dialogIds.Count} dialogs... it took {Stopwatch.GetElapsedTime(searchTagStartTimestamp)}");
        }));


        // Transmission:
        var (transmissionIds, transmissionCsvData) = Transmission.Generate(dialogIds, currentDate, intervalSeconds);

        tasks.Add(Task.Run(async () =>
        {
            var transmissionStartTimestamp = Stopwatch.GetTimestamp();
            await transmissionWriter.WriteAsync(transmissionCsvData);
            Console.WriteLine(
                $"Inserted {transmissionIds.Count} transmissions for {dialogIds.Count} dialogs... it took {Stopwatch.GetElapsedTime(transmissionStartTimestamp)}");
        }));


        // TransmissionContent:
        var (transmissionContentIds, transmissionContentCsvData) =
            TransmissionContent.Generate(transmissionIds, currentDate, intervalSeconds);

        tasks.Add(Task.Run(async () =>
        {
            var transmissionContentStartTimestamp = Stopwatch.GetTimestamp();
            await transmissionContentWriter.WriteAsync(transmissionContentCsvData);
            Console.WriteLine(
                $"Inserted {transmissionContentIds.Count} transmission content for {transmissionIds.Count} transmissions... it took {Stopwatch.GetElapsedTime(transmissionContentStartTimestamp)}");
        }));


        // Activity:
        var (activityIds, activityCsvData) = Activity.Generate(dialogIds, currentDate, intervalSeconds);

        tasks.Add(Task.Run(async () =>
        {
            var activityStartTimestamp = Stopwatch.GetTimestamp();
            await activityWriter.WriteAsync(activityCsvData);
            Console.WriteLine(
                $"Inserted {activityIds.Count} activities for {dialogIds.Count} dialogs... it took {Stopwatch.GetElapsedTime(activityStartTimestamp)}");
        }));


        // Attachment:
        var (attachmentIds, attachmentCsvData) =
            Attachment.Generate(dialogIds, transmissionIds, currentDate, intervalSeconds);

        tasks.Add(Task.Run(async () =>
        {
            var attachmentStartTimestamp = Stopwatch.GetTimestamp();
            await attachmentWriter.WriteAsync(attachmentCsvData);
            Console.WriteLine(
                $"Inserted {attachmentIds.Count} attachments for {dialogIds.Count} dialogs and {transmissionIds.Count} transmissions... it took {Stopwatch.GetElapsedTime(attachmentStartTimestamp)}");
        }));


        // AttachmentUrl:
        var attachmentUrlCsvData = AttachmentUrl.Generate(attachmentIds, currentDate, intervalSeconds);

        tasks.Add(Task.Run(async () =>
        {
            var attachmentUrlStartTimestamp = Stopwatch.GetTimestamp();
            await attachmentUrlWriter.WriteAsync(attachmentUrlCsvData);
            Console.WriteLine(
                $"Inserted attachment urls for {attachmentIds.Count} attachments... it took {Stopwatch.GetElapsedTime(attachmentUrlStartTimestamp)}");
        }));


        // Actor:
        var actorCsvData = Actor.Generate(activityIds, seenLogIds, transmissionIds, currentDate, intervalSeconds);

        tasks.Add(Task.Run(async () =>
        {
            var actorStartTimestamp = Stopwatch.GetTimestamp();
            await actorWriter.WriteAsync(actorCsvData);
            Console.WriteLine(
                $"Inserted actors for {dialogIds.Count} dialogs... it took {Stopwatch.GetElapsedTime(actorStartTimestamp)}");
        }));


        // LocalizationSet:
        var (localizationSetIds, localizationSetCsvData) = LocalizationSet.Generate(attachmentIds, guiActionIds,
            activityIds, dialogContentIds, transmissionContentIds, currentDate, intervalSeconds);

        tasks.Add(Task.Run(async () =>
        {
            var localizationSetStartTimestamp = Stopwatch.GetTimestamp();
            await localizationSetWriter.WriteAsync(localizationSetCsvData);
            Console.WriteLine(
                $"Inserted localization sets for {dialogIds.Count} dialogs... it took {Stopwatch.GetElapsedTime(localizationSetStartTimestamp)}");
        }));


        // Localization:
        var localizationCsvData = Localization.Generate(localizationSetIds, currentDate);

        tasks.Add(Task.Run(async () =>
        {
            var localizationStartTimestamp = Stopwatch.GetTimestamp();
            await localizationWriter.WriteAsync(localizationCsvData);
            Console.WriteLine(
                $"Inserted localizations for {localizationSetIds.Count} localization sets... it took {Stopwatch.GetElapsedTime(localizationStartTimestamp)}");
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
