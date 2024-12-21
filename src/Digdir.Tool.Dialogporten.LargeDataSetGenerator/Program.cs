using System.Diagnostics;
using Digdir.Tool.Dialogporten.LargeDataSetGenerator;
using Npgsql;
using Activity = Digdir.Tool.Dialogporten.LargeDataSetGenerator.Activity;

#pragma warning disable CA1305

try
{
    Console.WriteLine("Starting large data set generator...");

    var connString = Environment.GetEnvironmentVariable("CONN_STRING");

    var startingDate = DateTimeOffset.Parse(Environment.GetEnvironmentVariable("FROM_DATE")!);
    var endDate = DateTimeOffset.Parse(Environment.GetEnvironmentVariable("TO_DATE")!);
    var dialogAmount = int.Parse(Environment.GetEnvironmentVariable("DIALOG_AMOUNT")!);

    var totalDialogCreatedStartTimestamp = Stopwatch.GetTimestamp();

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
    await using var transmissionContentWriter = transmissionContentConnection.BeginTextImport(TransmissionContent.CopyCommand);

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

    var dto = new SeedDatabaseDto(startingDate, endDate, dialogAmount);


    // DIALOG:
    tasks.Add(Task.Run(async () =>
    {
        var dialogStartTimestamp = Stopwatch.GetTimestamp();

        foreach (var dialogTimestamp in dto.GetDialogTimestamps)
        {
            var dialogCsvData = Dialog.Generate(dialogTimestamp);
            await dialogWriter.WriteLineAsync(dialogCsvData);
        }

        Console.WriteLine($"Inserted dialogs in {Stopwatch.GetElapsedTime(dialogStartTimestamp)}");
    }));


    // Dialog Content:
    tasks.Add(Task.Run(async () =>
    {
        var dialogContentStartTimestamp = Stopwatch.GetTimestamp();

        foreach (var dialogTimestamp in dto.GetDialogTimestamps)
        {
            var dialogContentCsvData = DialogContent.Generate(dialogTimestamp);
            await dialogContentWriter.WriteAsync(dialogContentCsvData);
        }

        Console.WriteLine($"Inserted dialog content in {Stopwatch.GetElapsedTime(dialogContentStartTimestamp)}");
    }));


    // Dialog Gui Action:
    tasks.Add(Task.Run(async () =>
    {
        var dialogGuiActionStartTimestamp = Stopwatch.GetTimestamp();

        foreach (var dialogTimestamp in dto.GetDialogTimestamps)
        {
            var dialogGuiActionCsvData = GuiAction.Generate(dialogTimestamp);
            await guiActionWriter.WriteAsync(dialogGuiActionCsvData);
        }
        Console.WriteLine($"Inserted dialog gui actions in {Stopwatch.GetElapsedTime(dialogGuiActionStartTimestamp)}");
    }));


    // EndUserContext:
    tasks.Add(Task.Run(async () =>
    {
        var endUserContextStartTimestamp = Stopwatch.GetTimestamp();
        foreach (var dialogTimestamp in dto.GetDialogTimestamps)
        {
            var endUserContextCsvData = EndUserContext.Generate(dialogTimestamp);
            await endUserContextWriter.WriteLineAsync(endUserContextCsvData);
        }

        Console.WriteLine($"Inserted end user contexts in {Stopwatch.GetElapsedTime(endUserContextStartTimestamp)}");
    }));


    // SeenLog:
    tasks.Add(Task.Run(async () =>
    {
        var seenLogStartTimestamp = Stopwatch.GetTimestamp();
        foreach (var dialogTimestamp in dto.GetDialogTimestamps)
        {
            var seenLogCsvData = SeenLog.Generate(dialogTimestamp);
            await seenLogWriter.WriteLineAsync(seenLogCsvData);
        }

        Console.WriteLine($"Inserted seen logs in {Stopwatch.GetElapsedTime(seenLogStartTimestamp)}");
    }));


    // SearchTags:
    tasks.Add(Task.Run(async () =>
    {
        var searchTagStartTimestamp = Stopwatch.GetTimestamp();
        foreach (var dialogTimestamp in dto.GetDialogTimestamps)
        {
            var searchTagCsvData = SearchTags.Generate(dialogTimestamp);
            await searchTagWriter.WriteAsync(searchTagCsvData);
        }
        Console.WriteLine($"Inserted search tags in {Stopwatch.GetElapsedTime(searchTagStartTimestamp)}");
    }));


    // Transmission:
    tasks.Add(Task.Run(async () =>
    {
        var transmissionStartTimestamp = Stopwatch.GetTimestamp();
        foreach (var dialogTimestamp in dto.GetDialogTimestamps)
        {
            var transmissionCsvData = Transmission.Generate(dialogTimestamp);
            await transmissionWriter.WriteAsync(transmissionCsvData);
        }
        Console.WriteLine($"Inserted transmissions in {Stopwatch.GetElapsedTime(transmissionStartTimestamp)}");
    }));


    // TransmissionContent:
    tasks.Add(Task.Run(async () =>
    {
        var transmissionContentStartTimestamp = Stopwatch.GetTimestamp();
        foreach (var dialogTimestamp in dto.GetDialogTimestamps)
        {
            var transmissionContentCsvData = TransmissionContent.Generate(dialogTimestamp);
            await transmissionContentWriter.WriteAsync(transmissionContentCsvData);
        }

        Console.WriteLine($"Inserted transmission content in {Stopwatch.GetElapsedTime(transmissionContentStartTimestamp)}");
    }));


    // Activity:
    tasks.Add(Task.Run(async () =>
    {
        var activityStartTimestamp = Stopwatch.GetTimestamp();
        foreach (var dialogTimestamp in dto.GetDialogTimestamps)
        {
            var activityCsvData = Activity.Generate(dialogTimestamp);
            await activityWriter.WriteAsync(activityCsvData);
        }
        Console.WriteLine($"Inserted activities in {Stopwatch.GetElapsedTime(activityStartTimestamp)}");
    }));


    // Attachment:
    tasks.Add(Task.Run(async () =>
    {
        var attachmentStartTimestamp = Stopwatch.GetTimestamp();
        foreach (var dialogTimestamp in dto.GetDialogTimestamps)
        {
            var attachmentCsvData = Attachment.Generate(dialogTimestamp);
            await attachmentWriter.WriteAsync(attachmentCsvData);
        }

        Console.WriteLine($"Inserted attachments in {Stopwatch.GetElapsedTime(attachmentStartTimestamp)}");
    }));


    // AttachmentUrl:
    tasks.Add(Task.Run(async () =>
    {
        var attachmentUrlStartTimestamp = Stopwatch.GetTimestamp();
        foreach (var dialogTimestamp in dto.GetDialogTimestamps)
        {
            var attachmentUrlCsvData = AttachmentUrl.Generate(dialogTimestamp);
            await attachmentUrlWriter.WriteAsync(attachmentUrlCsvData);
        }
        Console.WriteLine($"Inserted attachment urls in {Stopwatch.GetElapsedTime(attachmentUrlStartTimestamp)}");
    }));


    // Actor:
    tasks.Add(Task.Run(async () =>
    {
        var actorStartTimestamp = Stopwatch.GetTimestamp();
        foreach (var dialogTimestamp in dto.GetDialogTimestamps)
        {
            var actorCsvData = Actor.Generate(dialogTimestamp);
            await actorWriter.WriteAsync(actorCsvData);
        }
        Console.WriteLine($"Inserted actors in {Stopwatch.GetElapsedTime(actorStartTimestamp)}");
    }));


    // LocalizationSet:
    tasks.Add(Task.Run(async () =>
    {
        var localizationSetStartTimestamp = Stopwatch.GetTimestamp();
        foreach (var dialogTimestamp in dto.GetDialogTimestamps)
        {
            var localizationSetCsvData = LocalizationSet.Generate(dialogTimestamp);
            await localizationSetWriter.WriteAsync(localizationSetCsvData);
        }

        Console.WriteLine(
            $"Inserted localization sets in {Stopwatch.GetElapsedTime(localizationSetStartTimestamp)}");
    }));


    // Localization:
    tasks.Add(Task.Run(async () =>
    {
        var localizationStartTimestamp = Stopwatch.GetTimestamp();
        foreach (var dialogTimestamp in dto.GetDialogTimestamps)
        {
            var localizationCsvData = Localization.Generate(dialogTimestamp);
            await localizationWriter.WriteAsync(localizationCsvData);
        }

        Console.WriteLine($"Inserted localizations in {Stopwatch.GetElapsedTime(localizationStartTimestamp)}");
    }));

    await Task.WhenAll(tasks);

    Console.WriteLine(string.Empty);
    Console.WriteLine(string.Empty);

    var timeItTook = Stopwatch.GetElapsedTime(totalDialogCreatedStartTimestamp);
    Console.WriteLine($"Generates {dialogAmount} in {timeItTook}");
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
}

// TODO: Re-enable all indexes and constraints
#pragma warning restore CA1305
