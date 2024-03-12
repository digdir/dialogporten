using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bogus;
using CommandLine;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Create;

namespace Digdir.Tool.Dialogporten.GenerateFakeData;

public static class Program
{
    private const int RefreshRateMs = 200; // How often the progress is updated
    private const int DialogsPerBatch = 20; // How many dialogs to generate per call DialogGenerator
    private const int BoundedCapacity = 300; // Max number of dialogs in the queue
    private const int Consumers = 4; // Number of consumers posting to the API
    private const string FailedDirectory = "failed"; // Directory to write failed requests to

    public static async Task Main(string[] args) => await Parser.Default.ParseArguments<Options>(args).WithParsedAsync(RunAsync);

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    private static int _dialogCounter;
    private static readonly Stopwatch _stopwatch = new();
    private static async Task RunAsync(Options options)
    {
        if (!options.Submit)
        {
            Randomizer.Seed = new Random(options.Seed);
            var dialogs = DialogGenerator.GenerateFakeDialogs(new Randomizer().Number(int.MaxValue), options.Count);
            var serialized = JsonSerializer.Serialize(dialogs, _jsonSerializerOptions);
            Console.WriteLine(serialized);
            return;
        }

        var dialogQueue = new BlockingCollection<(int, CreateDialogDto)>(BoundedCapacity);
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        using var client = new HttpClient();
        client.BaseAddress = new Uri(options.Url);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        Console.WriteLine($"Generating {options.Count} fake dialogs...");
        _stopwatch.Start();

        var producerTask = Task.Run(() => ProduceDialogs(options, dialogQueue, cancellationToken), cancellationToken);
        var progressTask = Task.Run(() => UpdateProgress(options, cancellationToken), cancellationToken);

        var consumerTasks = new List<Task>();
        for (var i = 0; i < Consumers; i++)
        {
            // ReSharper disable once AccessToDisposedClosure
            consumerTasks.Add(Task.Run(() =>
                ConsumeDialogsAndPost(options, dialogQueue, client, cancellationToken), cancellationToken));
        }

        await producerTask;
        foreach (var task in consumerTasks)
        {
            await task;
        }

        _stopwatch.Stop();
        await progressTask;
    }

    private static async Task UpdateProgress(Options options, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_dialogCounter == 0 || _stopwatch.ElapsedMilliseconds == 0)
            {
                await Task.Delay(RefreshRateMs, cancellationToken);
                continue;
            }

            Console.Write(
                "\rProgress: {0}/{1} dialogs created, {2:F1} dialogs/second.",
                _dialogCounter,
                options.Count,
                _dialogCounter / _stopwatch.Elapsed.TotalSeconds);

            await Task.Delay(RefreshRateMs, cancellationToken);
            if (_dialogCounter >= options.Count)
            {
                break;
            }
        }

        Console.WriteLine(
            "\r{0}/{1} dialogs created in {2:F1} seconds ({3:F1} dialogs/second).",
            _dialogCounter,
            options.Count,
            _stopwatch.Elapsed.TotalSeconds,
            _dialogCounter / _stopwatch.Elapsed.TotalSeconds);
    }

    private static void ProduceDialogs(Options options, BlockingCollection<(int, CreateDialogDto)> dialogQueue, CancellationToken cancellationToken)
    {
        Randomizer.Seed = new Random(options.Seed);
        var totalDialogs = options.Count;
        var dialogCounter = 0;

        while (dialogCounter < totalDialogs && !cancellationToken.IsCancellationRequested)
        {
            var dialogsToGenerate = Math.Min(DialogsPerBatch, totalDialogs - dialogCounter);
            var dialogs = DialogGenerator.GenerateFakeDialogs(new Randomizer().Number(int.MaxValue), dialogsToGenerate).Take(dialogsToGenerate);
            foreach (var dialog in dialogs)
            {
                dialogQueue.Add((dialogCounter + 1, dialog), cancellationToken);
                dialogCounter++;

                if (dialogCounter >= totalDialogs)
                    break;
            }
        }

        dialogQueue.CompleteAdding();
    }

    private static async Task ConsumeDialogsAndPost(Options options, BlockingCollection<(int, CreateDialogDto)> dialogQueue,
        HttpClient client, CancellationToken cancellationToken)
    {
        while (!dialogQueue.IsCompleted)
        {
            try
            {
                if (!dialogQueue.TryTake(out (int index, CreateDialogDto dialog) item, Timeout.Infinite,
                        cancellationToken))
                {
                    continue;
                }

                var json = JsonSerializer.Serialize(item.dialog, _jsonSerializerOptions);
                var content = new StringContent(json, Encoding.Unicode, "application/json");
                var response = await client.PostAsync(options.Url, content, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    await HandleFailedDialog(item, response);
                }

                Interlocked.Increment(ref _dialogCounter);
            }
            catch (Exception ex)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine($"\nException occurred while posting dialog: {ex.Message}");
                }
            }
        }
    }

    private static async Task HandleFailedDialog((int, object) item, HttpResponseMessage response)
    {
        Console.WriteLine($"\nFailed to post dialog: {response.StatusCode}");
        var result = await response.Content.ReadAsStringAsync();
        Console.WriteLine(result);
        var json = JsonSerializer.Serialize(item.Item2, _jsonSerializerOptions);
        var output = $"{FailedDirectory}/{item.Item1}.json";
        try
        {
            Directory.CreateDirectory(FailedDirectory);
            await File.WriteAllTextAsync(output, json);
            Console.WriteLine($"Wrote request payload to '{output}'");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to write request payload to '{output}': {e.Message}");
        }
    }
}

public class Options
{
    [Option('c', "count", Required = false, HelpText = "Number of fake dialogs to generate.")]
    public int Count { get; set; } = 1;

    [Option('s', "seed", Required = false, HelpText = "Seed for the random number generator.")]
    public int Seed { get; set; } = 1337;

    [Option('s', "submit", Required = false, HelpText = "Attempt to create the generated dialogs using service owner API.")]
    public bool Submit { get; set; } = false;

    [Option('u', "url", Required = false,
        Default = "https://localhost:7214/api/v1/serviceowner/dialogs",
        HelpText = "Service owner endpoint to post dialogs")]
    public string Url { get; set; } = null!;
}
